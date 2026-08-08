using BLE.Toolkit.Cache;

namespace BLE.Toolkit.Tests.Cache;

public class CleanableListTests
{
    [Fact]
    public void Add_IncreasesCount()
    {
        var list = new CleanableList<string>();

        Assert.True(list.Add("a"));
        Assert.True(list.Add("b"));
        Assert.False(list.Add("a"));

        Assert.Equal(2, list.Count);
        Assert.Equal(["a", "b"], list.Select(p => p.Value).ToList());
    }

    [Fact]
    public void Add_Null_IsIgnored()
    {
        var list = new CleanableList<string>();

        Assert.False(list.Add(null!));

        Assert.Empty(list);
    }

    [Fact]
    public void Contains_Remove_WorkAsExpected()
    {
        var list = new CleanableList<string>();
        list.Add("a");
        list.Add("b");
        list.Add("c");

        Assert.True(list.Contains("b"));
        Assert.False(list.Contains("missing"));

        Assert.True(list.Remove("b"));
        Assert.False(list.Contains("b"));
        Assert.False(list.Remove("b"));
        Assert.Equal(["a", "c"], list.Select(p => p.Value).ToList());
    }

    [Fact]
    public void Clear_RemovesAllItems()
    {
        var list = new CleanableList<int>();
        list.Add(1);
        list.Add(2);

        list.Clear();

        Assert.Empty(list);
    }

    [Fact]
    public void CopyTo_CopiesProxiesIntoArray()
    {
        var list = new CleanableList<int>();
        list.Add(10);
        list.Add(20);

        var array = new CachedProxy<int>[4];
        list.CopyTo(array, 1);

        Assert.Null(array[0]);
        Assert.Equal(10, array[1].Value);
        Assert.Equal(20, array[2].Value);
        Assert.Null(array[3]);
    }

    [Fact]
    public void CopyTo_NullArray_Throws()
    {
        var list = new CleanableList<int>();
        list.Add(1);

        Assert.Throws<ArgumentNullException>(() => list.CopyTo(null!, 0));
    }

    [Fact]
    public void Enumeration_ReturnsCachedProxy()
    {
        var list = new CleanableList<string>();
        list.Add("item");

        var proxy = Assert.Single(list);
        Assert.IsType<CachedProxy<string>>(proxy);
        Assert.Equal("item", proxy.Value);
        Assert.Equal(1, proxy.UsedStat);
    }
}
