using RZ.Foundation.Helpers;

namespace RZ.Foundation;

public class MemoirTest
{
    [Test]
    public async ValueTask MemoirDictSameKey() {
        var sideeffect = 0;
        int test(int x) {
            ++sideeffect;
            return x + sideeffect;
        }
        var memoir = Memoir.DictWith<int,int>(test);
        memoir(1);

        var result = memoir(1);

        await Assert.That(result).IsNotEqualTo(3);
        await Assert.That(result).IsEqualTo(2);
    }

    [Test]
    public async Task MemoirDictSameKeyMultithread(CancellationToken cancelToken) {
        var sideeffect = 0;
        int test(int x) {
            ++sideeffect;
            return x + sideeffect;
        }
        var memoir = Memoir.From(test, new Memoir.DictionaryCache<int,int>(), new Memoir.MultithreadLocker<int>());
        var startLine = new ManualResetEventSlim();
        var tasks = Enumerable.Range(1, 1000).Select(_ => Task.Run(() => {
            startLine.Wait(cancelToken);
            return memoir(1);
        })).ToArray();

        startLine.Set();    // go!

        await Task.WhenAll(tasks.Cast<Task>().ToArray());

#pragma warning disable xUnit1031
        var results = tasks.Select(t => t.Result).ToArray();
#pragma warning restore xUnit1031

        await Assert.That(results.All(r => r == 2)).IsTrue();
        await Assert.That(sideeffect).IsEqualTo(1);
    }
}