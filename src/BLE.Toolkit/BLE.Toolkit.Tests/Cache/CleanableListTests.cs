using BLE.Toolkit.Cache;

namespace BLE.Toolkit.Tests.Cache;

public class CleanableListTests
{
    [Fact]
    public void Constructor_NonPositiveTimeout_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new CleanableList<int>(TimeSpan.Zero));
        Assert.Throws<ArgumentOutOfRangeException>(() => new CleanableList<int>(TimeSpan.FromSeconds(-1)));
        Assert.Throws<ArgumentOutOfRangeException>(() => new CleanableList<int>(0));
    }

    [Fact]
    public void Add_IncreasesCount()
    {
        var list = new CleanableList<string>(TimeSpan.FromMinutes(1));

        Assert.True(list.Add("a"));
        Assert.True(list.Add("b"));
        Assert.False(list.Add("a"));

        Assert.Equal(2, list.Count);
        Assert.Equal(["a", "b"], list.Select(p => p.Value).ToList());
    }

    [Fact]
    public void Add_Null_IsIgnored()
    {
        var list = new CleanableList<string>(TimeSpan.FromMinutes(1));

        Assert.False(list.Add(null!));

        Assert.Empty(list);
    }

    [Fact]
    public void Contains_Remove_WorkAsExpected()
    {
        var list = new CleanableList<string>(TimeSpan.FromMinutes(1));
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
        var list = new CleanableList<int>(TimeSpan.FromMinutes(1));
        list.Add(1);
        list.Add(2);

        list.Clear();

        Assert.Empty(list);
    }

    [Fact]
    public void CopyTo_CopiesProxiesIntoArray()
    {
        var list = new CleanableList<int>(TimeSpan.FromMinutes(1));
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
        var list = new CleanableList<int>(TimeSpan.FromMinutes(1));
        list.Add(1);

        Assert.Throws<ArgumentNullException>(() => list.CopyTo(null!, 0));
    }

    [Fact]
    public void Enumeration_ReturnsCachedProxy()
    {
        var list = new CleanableList<string>(TimeSpan.FromMinutes(1));
        list.Add("item");

        var proxy = Assert.Single(list);
        Assert.IsType<CachedProxy<string>>(proxy);
        Assert.Equal("item", proxy.Value);
        Assert.Equal(1, proxy.UsedStat);
    }

    [Fact]
    public void ItemsExpire_AfterTimeout()
    {
        var now = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var list = new CleanableList<string>(TimeSpan.FromSeconds(10), () => now);

        list.Add("alive");
        Assert.Single(list);

        now = now.AddSeconds(11);

        Assert.Empty(list);
        Assert.False(list.Contains("alive"));
    }

    [Fact]
    public void ExpiredItemsArePurged_OnAdd()
    {
        var now = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var list = new CleanableList<string>(TimeSpan.FromSeconds(5), () => now);

        list.Add("old");
        now = now.AddSeconds(6);
        list.Add("new");

        Assert.Equal(["new"], list.Select(p => p.Value).ToList());
    }

    [Fact]
    public void ExpiredItemsAreNotPurged_OnRead()
    {
        var now = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var list = new CleanableList<string>(TimeSpan.FromSeconds(5), () => now);

        list.Add("old");
        now = now.AddSeconds(6);

        _ = list.Count;
        _ = list.Contains("old");
        Assert.Equal([], list.Select(p => p.Value).ToList());

        list.Add("new");

        Assert.Equal(["new"], list.Select(p => p.Value).ToList());
    }

    [Fact]
    public void ReAdd_RefreshesExpiration()
    {
        var now = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var list = new CleanableList<string>(TimeSpan.FromSeconds(10), () => now);

        list.Add("item");
        now = now.AddSeconds(9);

        Assert.False(list.Add("item"));

        now = now.AddSeconds(5);
        Assert.Equal("item", Assert.Single(list).Value);
    }

    [Fact]
    public void SecondsConstructor_UsesTimeoutInSeconds()
    {
        var now = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var list = new CleanableList<int>(2, () => now);

        list.Add(1);
        now = now.AddSeconds(1);
        Assert.Single(list);

        now = now.AddSeconds(2);
        Assert.Empty(list);
    }
}
