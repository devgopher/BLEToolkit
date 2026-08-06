using BLE.Toolkit.Utils;

namespace BLE.Toolkit.Tests.Utils;

public class ListUtilsTests
{
    [Fact]
    public void AddRange_AppendsAllItems()
    {
        IList<int> list = [1];

        list.AddRange([2, 3, 4]);

        Assert.Equal([1, 2, 3, 4], list);
    }

    [Fact]
    public void AddRange_NullCollection_DoesNothing()
    {
        IList<string> list = ["a"];

        list.AddRange(null);

        Assert.Equal(["a"], list);
    }

    [Fact]
    public void AddRange_EmptyCollection_DoesNothing()
    {
        IList<int> list = [1];

        list.AddRange([]);

        Assert.Equal([1], list);
    }
}
