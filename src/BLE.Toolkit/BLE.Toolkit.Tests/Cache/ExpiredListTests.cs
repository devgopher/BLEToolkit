using BLE.Toolkit.Cache;

namespace BLE.Toolkit.Tests.Cache;

public class ExpiredListTests
{
    [Fact]
    public void Constructor_NonPositiveTimeout_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ExpiredList<int>(TimeSpan.Zero));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ExpiredList<int>(TimeSpan.FromSeconds(-1)));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ExpiredList<int>(0));
    }

    [Fact]
    public void Add_IncreasesCount()
    {
        var list = new ExpiredList<string>(TimeSpan.FromMinutes(1));

        list.Add("a");
        list.Add("b");

        Assert.Equal(2, list.Count);
        Assert.Equal(["a", "b"], list.ToList());
    }

    [Fact]
    public void Add_Null_IsIgnored()
    {
        var list = new ExpiredList<string>(TimeSpan.FromMinutes(1));

        list.Add(null!);

        Assert.Empty(list);
    }

    [Fact]
    public void Contains_IndexOf_Remove_WorkAsExpected()
    {
        var list = new ExpiredList<string>(TimeSpan.FromMinutes(1));
        list.Add("a");
        list.Add("b");
        list.Add("c");

        Assert.Contains("b", list);
        Assert.Equal(1, list.IndexOf("b"));
        Assert.Equal(-1, list.IndexOf("missing"));

        Assert.True(list.Remove("b"));
        Assert.DoesNotContain("b", list);
        Assert.False(list.Remove("b"));
        Assert.Equal(["a", "c"], list.ToList());
    }

    [Fact]
    public void Insert_RemoveAt_AndIndexer_WorkAsExpected()
    {
        var list = new ExpiredList<string>(TimeSpan.FromMinutes(1));
        list.Add("a");
        list.Add("c");

        list.Insert(1, "b");
        Assert.Equal(["a", "b", "c"], list.ToList());

        Assert.Equal("b", list[1]);
        list[1] = "B";
        Assert.Equal("B", list[1]);

        list.RemoveAt(1);
        Assert.Equal(["a", "c"], list.ToList());
    }

    [Fact]
    public void Clear_RemovesAllItems()
    {
        var list = new ExpiredList<int>(TimeSpan.FromMinutes(1));
        list.Add(1);
        list.Add(2);

        list.Clear();

        Assert.Empty(list);
    }

    [Fact]
    public void CopyTo_CopiesItemsIntoArray()
    {
        var list = new ExpiredList<int>(TimeSpan.FromMinutes(1));
        list.Add(10);
        list.Add(20);

        var array = new int[4];
        list.CopyTo(array, 1);

        Assert.Equal([0, 10, 20, 0], array);
    }

    [Fact]
    public void CopyTo_NullArray_Throws()
    {
        var list = new ExpiredList<int>(TimeSpan.FromMinutes(1));
        list.Add(1);

        Assert.Throws<ArgumentNullException>(() => list.CopyTo(null!, 0));
    }

    [Fact]
    public void ItemsExpire_AfterTimeout()
    {
        var now = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var list = new ExpiredList<string>(TimeSpan.FromSeconds(10), () => now);

        list.Add("alive");
        Assert.Single(list);

        now = now.AddSeconds(11);

        Assert.Empty(list);
        Assert.DoesNotContain("alive", list);
    }

    [Fact]
    public void IndexerGet_RefreshesExpiration()
    {
        var now = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var list = new ExpiredList<string>(TimeSpan.FromSeconds(10), () => now);

        list.Add("item");
        now = now.AddSeconds(9);

        _ = list[0];

        now = now.AddSeconds(5);
        Assert.Equal("item", Assert.Single(list));
    }

    [Fact]
    public void IndexerSet_RefreshesExpiration()
    {
        var now = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var list = new ExpiredList<string>(TimeSpan.FromSeconds(10), () => now);

        list.Add("old");
        now = now.AddSeconds(9);

        list[0] = "new";

        now = now.AddSeconds(5);
        Assert.Equal(["new"], list.ToList());
    }

    [Fact]
    public void ExpiredItemsArePurged_OnAdd()
    {
        var now = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var list = new ExpiredList<string>(TimeSpan.FromSeconds(5), () => now);

        list.Add("old");
        now = now.AddSeconds(6);
        list.Add("new");

        Assert.Equal(["new"], list.ToList());
    }

    [Fact]
    public void SecondsConstructor_UsesTimeoutInSeconds()
    {
        var now = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var list = new ExpiredList<int>(2, () => now);

        list.Add(1);
        now = now.AddSeconds(1);
        Assert.Single(list);

        now = now.AddSeconds(2);
        Assert.Empty(list);
    }

    [Fact]
    public void IsReadOnly_IsFalse()
    {
        var list = new ExpiredList<int>(TimeSpan.FromMinutes(1));
        Assert.False(list.IsReadOnly);
    }
}
