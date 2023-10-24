﻿namespace MaiLib;
using static MaiLib.NoteEnum;

/// <summary>
///     Construct a Slide note (With START!)
/// </summary>
public class Slide : Note
{
    #region Constructors
    /// <summary>
    ///     Empty Constructor
    /// </summary>
    public Slide() { }

    /// <summary>
    ///     Construct a Slide Note (Valid only if Start Key matches a start!)
    /// </summary>
    /// <param name="noteType">
    ///     SI_(Straight),SCL,SCR,SV_(Line not intercepting Crossing Center),SUL,SUR,SF_(Wifi),SLL(Infecting
    ///     Line),SLR(Infecting),SXL(Self winding),SXR(Self winding),SSL,SSR
    /// </param>
    /// <param name="key">0-7</param>
    /// <param name="bar">Bar in</param>
    /// <param name="startTime">Start Time</param>
    /// <param name="lastTime">Last Time</param>
    /// <param name="endKey">0-7</param>
    public Slide(NoteType noteType, int bar, int startTime, string key, int waitTime, int lastTime, string endKey)
    {
        NoteType = noteType;
        Key = key;
        Bar = bar;
        Tick = startTime;
        WaitLength = waitTime;
        LastLength = lastTime;
        EndKey = endKey;
        Delayed = WaitLength != 96;
        Update();
    }

    /// <summary>
    ///     Construct a Slide from another note
    /// </summary>
    /// <param name="inTake">The intake note</param>
    public Slide(Note inTake)
    {
        inTake.CopyOver(this);
    }
    #endregion

    public override NoteGenre NoteGenre => NoteGenre.SLIDE;

    public override bool IsNote => true;

    public override NoteSpecificGenre NoteSpecificGenre => NoteSpecificGenre.SLIDE;

    //TODO: REWRITE THIS
    public override bool CheckValidity()
    {
        return true;
    }

    public override string Compose(int format)
    {
        var result = "";
        if (format == 1)
        {
            result = NoteType + "\t" + Bar + "\t" + Tick + "\t" + Key + "\t" + WaitLength + "\t" + LastLength + "\t" +
                     EndKey;
        }
        else if (format == 0)
        {
            switch (NoteType)
            {
                case NoteType.SI_:
                    result += "-";
                    break;
                case NoteType.SV_:
                    result += "v";
                    break;
                case NoteType.SF_:
                    result += "w";
                    break;
                case NoteType.SCL:
                    if (int.Parse(Key) == 0 || int.Parse(Key) == 1 || int.Parse(Key) == 6 || int.Parse(Key) == 7)
                        result += "<";
                    else
                        result += ">";
                    break;
                case NoteType.SCR:
                    if (int.Parse(Key) == 0 || int.Parse(Key) == 1 || int.Parse(Key) == 6 || int.Parse(Key) == 7)
                        result += ">";
                    else
                        result += "<";
                    break;
                case NoteType.SUL:
                    result += "p";
                    break;
                case NoteType.SUR:
                    result += "q";
                    break;
                case NoteType.SSL:
                    result += "s";
                    break;
                case NoteType.SSR:
                    result += "z";
                    break;
                case NoteType.SLL:
                    result += "V" + GenerateInflection(this);
                    break;
                case NoteType.SLR:
                    result += "V" + GenerateInflection(this);
                    break;
                case NoteType.SXL:
                    result += "pp";
                    break;
                case NoteType.SXR:
                    result += "qq";
                    break;
            }

            result += (Convert.ToInt32(EndKey) + 1).ToString();
            if (NoteSpecialState == SpecialState.Break)
                result += "b";
            else if (NoteSpecialState == SpecialState.EX)
                result += "x";
            else if (NoteSpecialState == SpecialState.BreakEX) result += "bx";
            if (TickBPMDisagree || Delayed)
            {
                //result += GenerateAppropriateLength(this.LastLength, this.BPM);
                if (NoteSpecialState != SpecialState.ConnectingSlide)
                    result += GenerateAppropriateLength(LastLength, BPM);
                else result += GenerateAppropriateLength(FixedLastLength);
            }
            else
            {
                result += GenerateAppropriateLength(LastLength);
            }
            //result += "_" + this.Tick;
            //result += "_" + this.Key;
        }

        return result;
    }

    /// <summary>
    ///     Return inflection point of SLL and SLR
    /// </summary>
    /// <param name="x">This note</param>
    /// <returns>Infection point of this note</returns>
    public static int GenerateInflection(Note x)
    {
        var result = int.Parse(x.Key) + 1;
        if (x.NoteType is NoteType.SLR)
            result += 2;
        else if (x.NoteType is NoteType.SLL) result -= 2;

        if (result > 8)
            result -= 8;
        else if (result < 1) result += 8;

        if (result == int.Parse(x.Key) + 1 || result == int.Parse(x.EndKey) + 1)
        {
            //Deal with result;
            if (result > 4)
                result -= 4;
            else if (result <= 4) result += 4;

            //Deal with note type;
            if (x.NoteType is NoteType.SLL)
                x.NoteType = NoteType.SLR;
            else if (x.NoteType is NoteType.SLR)
                x.NoteType = NoteType.SLL;
            else
                throw new InvalidDataException("INFLECTION POINT IS THE SAME WITH ONE OF THE KEY!");
        }

        return result;
    }

    public override Note NewInstance()
    {
        Note result = new Slide(this);
        return result;
    }
}
