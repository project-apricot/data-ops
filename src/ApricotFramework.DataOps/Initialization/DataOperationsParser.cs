using System.Globalization;
using System.Xml;
using System.Xml.Schema;
using ApricotFramework.DataOps.Definitions;

namespace ApricotFramework.DataOps.Initialization;

/// <summary>
/// Reads an operations definition document into the definition model.
/// </summary>
public class DataOperationsParser
{
    private const string SchemaResourceName = "ApricotFramework.DataOps.DataOps.xsd";

    private static readonly char[] ProviderSeparators = [' ', '\t', '\r', '\n', ','];

    private readonly bool validate;

    private XmlSchema? validationSchema;

    /// <summary>
    /// Creates a parser.
    /// </summary>
    /// <param name="validate">Whether to check the document against the published schema.</param>
    public DataOperationsParser(bool validate = false)
    {
        this.validate = validate;
    }

    /// <summary>
    /// Reads a definition document from a file.
    /// </summary>
    /// <param name="xml">The path to the document.</param>
    /// <returns>The definition.</returns>
    public DataOperationsDefinition Parse(string xml)
    {
        using var stream = new FileStream(xml, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        return this.Parse(stream);
    }

    /// <summary>
    /// Reads a definition document from a stream.
    /// </summary>
    /// <param name="xml">The stream to read.</param>
    /// <returns>The definition.</returns>
    public DataOperationsDefinition Parse(Stream xml)
    {
        var document = LoadDocument(xml);

        if (this.validate)
        {
            this.ValidateDocument(document);
        }

        return ParseDocument(document);
    }

    private static XmlDocument LoadDocument(Stream xml)
    {
        // No resolver and no DTD, so a definition file cannot pull in anything from disk or the network.
        var settings = new XmlReaderSettings
        {
            XmlResolver = null,
            DtdProcessing = DtdProcessing.Prohibit
        };

        var document = new XmlDocument { XmlResolver = null };

        try
        {
            using var reader = XmlReader.Create(xml, settings);
            document.Load(reader);
        }
        catch (XmlException exception)
        {
            throw new OperationParsingException($"The operations definition is not well-formed XML: {exception.Message}", exception);
        }

        return document;
    }

    private void ValidateDocument(XmlDocument document)
    {
        var schema = this.GetValidationSchema();

        if (!document.Schemas.Contains(schema))
        {
            document.Schemas.Add(schema);
        }

        var errors = new List<string>();

        // Collecting through the handler rather than letting the reader throw, so one pass reports
        // every problem in the file instead of only the first.
        document.Validate((_, args) =>
        {
            if (args.Severity == XmlSeverityType.Error)
            {
                errors.Add(args.Message);
            }
        });

        if (errors.Count != 0)
        {
            throw new OperationParsingException(string.Join(Environment.NewLine, errors));
        }
    }

    private XmlSchema GetValidationSchema()
    {
        if (this.validationSchema is not null)
        {
            return this.validationSchema;
        }

        using var xsdStream = typeof(DataOperationsParser).Assembly.GetManifestResourceStream(SchemaResourceName)
            ?? throw new OperationParsingException($"The validation schema '{SchemaResourceName}' is missing from the assembly.");

        using var xsdReader = XmlReader.Create(xsdStream, new XmlReaderSettings
        {
            XmlResolver = null,
            DtdProcessing = DtdProcessing.Prohibit
        });

        this.validationSchema = XmlSchema.Read(xsdReader, null)
            ?? throw new OperationParsingException($"The validation schema '{SchemaResourceName}' could not be read.");

        return this.validationSchema;
    }

    private static DataOperationsDefinition ParseDocument(XmlDocument document)
    {
        var result = new DataOperationsDefinition();

        var config = document.GetElementsByTagName("DataConfiguration").OfType<XmlElement>().FirstOrDefault();

        if (config is not null)
        {
            result.Configuration = MapConfiguration(config);
        }

        foreach (var group in document.GetElementsByTagName("OperationGroup").OfType<XmlElement>())
        {
            result.Groups.Add(MapGroup(group));
        }

        return result;
    }

    private static OperationGroup MapGroup(XmlElement element)
    {
        var group = new OperationGroup
        {
            Name = element.Attributes["Name"]?.Value
        };

        MapTransactionOptionsTo(element, group);
        MapProviderOptionsTo(element, group);

        foreach (var operation in element.GetElementsByTagName("SqlOperation").OfType<XmlElement>())
        {
            group.Operations.Add(MapOperation(group, operation));
        }

        return group;
    }

    private static SqlOperationDefinition MapOperation(OperationGroup group, XmlElement element)
    {
        var operation = new SqlOperationDefinition
        {
            Name = element.Attributes["Name"]?.Value,
            Group = group
        };

        var command = element.ChildNodes.OfType<XmlElement>().FirstOrDefault();

        if (command is null)
        {
            throw new OperationParsingException($"The operation '{group.Name}/{operation.Name}' declares no command.");
        }

        operation.Command = MapCommand(command);

        MapTransactionOptionsTo(element, operation);
        MapProviderOptionsTo(element, operation);

        return operation;
    }

    private static CommandDefinition MapCommand(XmlElement element)
    {
        var type = element.Name switch
        {
            "StoredProcedure" => OperationType.StoredProcedure,
            "TextCommand" => OperationType.Text,
            _ => OperationType.Unknown
        };

        // A stored procedure carries its name in an attribute; anything else carries its SQL as text.
        var source = type == OperationType.StoredProcedure
            ? element.Attributes["Name"]?.Value
            : element.InnerText.Trim();

        return new CommandDefinition
        {
            Source = source,
            Type = type,
            ExpectedResult = ParseEnum(element.Attributes["ExpectedResult"]?.Value, ResultType.Unknown)
        };
    }

    private static DataConfiguration MapConfiguration(XmlElement element)
    {
        var config = new DataConfiguration();

        MapTransactionOptionsTo(element, config);
        MapProviderOptionsTo(element, config);

        return config;
    }

    private static void MapTransactionOptionsTo(XmlElement element, ITransactionOptions options)
    {
        var value = element.Attributes["AutoTransaction"]?.Value;

        if (TryParseEnum<AutoTransaction>(value, out var autoTransaction))
        {
            options.AutoTransaction = autoTransaction;
        }
    }

    private static void MapProviderOptionsTo(XmlElement element, IProviderOptions options)
    {
        var providers = ParseProviders(element.Attributes["Compatibility"]?.Value);

        if (providers.HasValue)
        {
            options.Providers = providers.Value;
        }

        if (TryParseTimeout(element.Attributes["Timeout"]?.Value, out var timeout))
        {
            options.Timeout = timeout;
        }
    }

    /// <summary>
    /// Reads the Compatibility attribute, which the schema declares as a whitespace-separated list;
    /// commas are accepted too, since that is what the enum syntax looks like.
    /// </summary>
    private static CompatibilityProviders? ParseProviders(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var tokens = value.Split(ProviderSeparators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var result = CompatibilityProviders.None;

        foreach (var token in tokens)
        {
            if (!TryParseEnum<CompatibilityProviders>(token, out var provider))
            {
                throw new OperationParsingException($"'{token}' is not a provider the Compatibility attribute accepts.");
            }

            result |= provider;
        }

        return result;
    }

    private static TEnum ParseEnum<TEnum>(string? value, TEnum fallback) where TEnum : struct, Enum
    {
        return TryParseEnum<TEnum>(value, out var parsed) ? parsed : fallback;
    }

    /// <summary>
    /// Parses a member name case-insensitively, rejecting the numeric form so that a typo in a
    /// definition file cannot be read as an arbitrary enum value.
    /// </summary>
    private static bool TryParseEnum<TEnum>(string? value, out TEnum parsed) where TEnum : struct, Enum
    {
        parsed = default;

        if (string.IsNullOrWhiteSpace(value) || char.IsAsciiDigit(value.Trim()[0]))
        {
            return false;
        }

        return Enum.TryParse(value.Trim(), ignoreCase: true, out parsed);
    }

    private static bool TryParseTimeout(string? value, out TimeSpan result)
    {
        result = default;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        // The schema calls for an xs:duration such as PT5M, but a plain TimeSpan is accepted too.
        if (TimeSpan.TryParse(value, CultureInfo.InvariantCulture, out result))
        {
            return true;
        }

        try
        {
            result = XmlConvert.ToTimeSpan(value);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
