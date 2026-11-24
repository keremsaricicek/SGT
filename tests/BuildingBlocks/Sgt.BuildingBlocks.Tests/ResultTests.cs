using Sgt.BuildingBlocks;

namespace Sgt.BuildingBlocks.Tests;

public sealed class ResultTests
{
    private static readonly Error SampleError = new("Sample.Failed", "Örnek hata");

    [Fact]
    public void Success_has_no_error()
    {
        var result = Result.Success();

        Assert.True(result.IsSuccess);
        Assert.Equal(Error.None, result.Error);
    }

    [Fact]
    public void Error_converts_to_failed_result()
    {
        Result result = SampleError;

        Assert.True(result.IsFailure);
        Assert.Equal(SampleError, result.Error);
    }

    [Fact]
    public void Value_converts_to_successful_typed_result()
    {
        Result<int> result = 42;

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void Value_of_failed_result_cannot_be_read()
    {
        Result<int> result = SampleError;

        Assert.Throws<InvalidOperationException>(() => result.Value);
    }

    [Fact]
    public void Failure_requires_a_real_error()
    {
        Assert.Throws<ArgumentException>(() => Result.Failure(Error.None));
    }
}
