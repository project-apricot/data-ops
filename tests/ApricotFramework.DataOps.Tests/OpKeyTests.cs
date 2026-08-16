namespace ApricotFramework.DataOps.Tests;

public class OpKeyTests
{
    [Fact]
    public void Equals_SameGroupAndName_IsTrue()
    {
        Assert.Equal(OpKey.Of("People", "All"), OpKey.Of("People", "All"));
        Assert.True(OpKey.Of("People", "All") == OpKey.Of("People", "All"));
    }

    [Fact]
    public void Equals_DifferentCase_IsFalse()
    {
        Assert.NotEqual(OpKey.Of("People", "All"), OpKey.Of("people", "all"));
        Assert.True(OpKey.Of("People", "All") != OpKey.Of("people", "all"));
    }

    [Fact]
    public void Equals_GroupAndNameSwapped_IsFalse()
    {
        Assert.NotEqual(OpKey.Of("a", "b"), OpKey.Of("b", "a"));
    }

    [Fact]
    public void GetHashCode_EqualKeys_Match()
    {
        Assert.Equal(OpKey.Of("People", "All").GetHashCode(), OpKey.Of("People", "All").GetHashCode());
    }

    [Fact]
    public void Of_NullParts_BecomeEmptyStrings()
    {
        var key = OpKey.Of(null, null);

        Assert.Equal(string.Empty, key.Group);
        Assert.Equal(string.Empty, key.Name);
    }

    [Fact]
    public void Of_NoGroup_MatchesAnEmptyGroup()
    {
        Assert.Equal(OpKey.Of(string.Empty, "All"), OpKey.Of("All"));
    }

    [Fact]
    public void ToString_RendersAsGroupSlashName()
    {
        Assert.Equal("People/All", OpKey.Of("People", "All").ToString());
    }
}
