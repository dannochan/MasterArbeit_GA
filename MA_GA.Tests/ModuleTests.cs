using FluentAssertions;
using MA_GA.domain.module;
using Xunit;

namespace MA_GA.Tests;

public class ModuleTests
{
    [Fact]
    public void NewModule_CheckIndex_ReturnsFalse()
    {
        var module = new Module();
        module.CheckIndexInModule(0).Should().BeFalse();
    }

    [Fact]
    public void AddIndex_IndexIsPresent()
    {
        var module = new Module();
        module.AddIndex(5);
        module.CheckIndexInModule(5).Should().BeTrue();
    }

    [Fact]
    public void AddIndex_MaintainsSortedOrder()
    {
        var module = new Module();
        module.AddIndex(3);
        module.AddIndex(1);
        module.AddIndex(7);
        module.AddIndex(0);

        module.GetIndices().Should().BeInAscendingOrder();
    }

    [Fact]
    public void AddMultipleIndices_AllPresent()
    {
        var module = new Module();
        module.AddIndex(0);
        module.AddIndex(1);
        module.AddIndex(2);

        module.GetIndices().Should().BeEquivalentTo(new[] { 0, 1, 2 });
    }

    [Fact]
    public void RemoveIndex_IndexIsAbsent()
    {
        var module = new Module();
        module.AddIndex(4);
        module.AddIndex(8);

        module.RemoveIndex(4);

        module.CheckIndexInModule(4).Should().BeFalse();
        module.CheckIndexInModule(8).Should().BeTrue();
    }

    [Fact]
    public void RemoveIndex_NonExistentIndex_ThrowsArgumentException()
    {
        var module = new Module();
        module.AddIndex(1);

        var act = () => module.RemoveIndex(99);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GetAlleleOfEndingNode_ReturnsLastIndex()
    {
        var module = new Module();
        module.AddIndex(2);
        module.AddIndex(5);
        module.AddIndex(9);

        module.GetAlleleOfEndingNode().Should().Be(9);
    }

    [Fact]
    public void GetAlleleOfEndingNode_EmptyModule_ThrowsInvalidOperationException()
    {
        var module = new Module();

        var act = () => module.GetAlleleOfEndingNode();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Clone_ProducesIndependentCopy()
    {
        var original = new Module();
        original.AddIndex(0);
        original.AddIndex(3);

        var clone = original.Clone();
        clone.AddIndex(7);

        original.CheckIndexInModule(7).Should().BeFalse();
        clone.CheckIndexInModule(7).Should().BeTrue();
    }

    [Fact]
    public void Equals_SameIndices_AreEqual()
    {
        var m1 = new Module();
        m1.AddIndex(1);
        m1.AddIndex(3);

        var m2 = new Module();
        m2.AddIndex(1);
        m2.AddIndex(3);

        m1.Should().Be(m2);
    }

    [Fact]
    public void Equals_DifferentIndices_AreNotEqual()
    {
        var m1 = new Module();
        m1.AddIndex(1);

        var m2 = new Module();
        m2.AddIndex(2);

        m1.Should().NotBe(m2);
    }

    [Fact]
    public void AddIndices_HashSet_AddsAll()
    {
        var module = new Module();
        var indices = new HashSet<object> { 10, 20, 30 };

        module.AddIndices(indices);

        module.GetIndices().Should().BeEquivalentTo(new[] { 10, 20, 30 });
    }
}
