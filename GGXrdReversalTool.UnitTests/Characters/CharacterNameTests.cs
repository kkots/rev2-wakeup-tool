using GGXrdReversalTool.Library.Domain.Characters;

namespace GGXrdReversalTool.UnitTests.Characters;

public class CharacterNameTests
{
    [Fact]
    public void Given_The_Same_CharacterName_The_Objects_Should_Be_Equal()
    {
        var charName1 = new CharacterName("toto");
        var charName2 = new CharacterName("toto");

        Assert.Equal(charName1, charName2);
    }

    [Fact]
    public void Given_An_Empty_String_As_CharacterName_It_Should_Throw_ArgumentException()
    {

        Assert.Throws<ArgumentException>(() => new CharacterName(""));
        Assert.Throws<ArgumentException>(() => new CharacterName("  "));
        Assert.Throws<ArgumentException>(() => new CharacterName(null!));
    }
}