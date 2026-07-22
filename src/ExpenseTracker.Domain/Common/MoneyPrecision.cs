namespace ExpenseTracker.Domain.Common;

internal static class MoneyPrecision
{
    private const int ScaleMask = 0x00FF0000;
    private const int ScaleShift = 16;

    internal static bool HasAtMostTwoDecimalPlaces(decimal value)
    {
        /*
        This function zooms into the hidden, internal "blueprint" of a decimal number to find out exactly how many digits are after the decimal point.
        Here is exactly how it pulls off that magic trick:
        Under the hood, C# stores every decimal as a massive whole number, plus a hidden "scale" indicator that says where to put the dot.
        Take 19.99 for instance. The "big number" is 1999, and the scale is 2, meaning: "Move the dot 2 spaces from right to left".

        To access this hidden metadata, we use the decimal.GetBits() method, which gives us an array of four integers (32-bit blocks) that represent the decimal's internal structure.
        The first three integers are the "big number" part, so we can ignore them. The fourth integer is where the scale and sign are stored, packed tightly together.
        To isolate the scale (i.e. ignore the sign), we can use a technique called Bitmasking:

        const int scaleMask = 0x00FF0000;
        decimal.GetBits(value)[3] & scaleMask;

        The scaleMask is like as a cardboard cutout with a single slot cut into it.
        When placed over the fourth integer using the & symbol, it blacks out everything else and only lets us see the tiny slot where the scale number is written.

        Now, again, integers as 32-bit blocks. Even though we've hidden the junk, the scale we need is sitting far to the left, in slots 16 to 23.
        That means there are exactly 16 unused slots to the right of where the scale is.
        If we asked C# to read the block, it'd see a massive number because those 16 empty slots at the front act like trailing zeros.
        For a scale of 2, it'd instead read 131,072. That's when we use a "right-shift":

        const int scaleShift = 16;
        (decimal.GetBits(value)[3] & scaleMask) >> scaleShift;

        The ">> 16" command tells C#: "Grab the entire train of slots, pull it 16 steps to the right, and throw away whatever falls off the end."
        In other words, after the shift, everything slides down by 16 spaces, so slots 16-23 slide down into slots 0-7.
        Now that the scale data is sitting right at the very beginning of the train, When C# reads the number, it ignores the empty space behind it and reads it as a perfectly normal, clean integer (ideally 2).
        */

        var flags = decimal.GetBits(value)[3];
        var scale = (flags & ScaleMask) >> ScaleShift;

        return scale <= 2;
    }
}
