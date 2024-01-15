﻿using System.Diagnostics;
using System.Reflection;
using static MaiLib.NoteEnum;

namespace MaiLib;

/// <summary>
///     Compile various Ma2 Charts
/// </summary>
public class SimaiCompiler : Compiler
{
    public bool StrictDecimalLevel { get; set; }

    public string Result { get; private set; }

    /// <summary>
    ///     Construct compiler of a single song.
    /// </summary>
    /// <param name="location">Folder</param>
    /// <param name="targetLocation">Output folder</param>
    public SimaiCompiler(bool strictDecimal, string location, string targetLocation)
    {
        StrictDecimalLevel = strictDecimal;
        for (var i = 0; i < 7; i++) Charts.Add(new Simai());
        MusicXML = new XmlInformation(location);
        Information = MusicXML.Information;
        //Construct Charts
        {
            if (!Information["Easy"].Equals("") &&
                File.Exists(location + Information.GetValueOrDefault("Easy Chart Path")))
                Charts[0] = new Ma2(location + Information.GetValueOrDefault("Easy Chart Path"));
            if (!Information["Basic"].Equals("") &&
                File.Exists(location + Information.GetValueOrDefault("Basic Chart Path")))
                //Console.WriteLine("Have basic: "+ location + this.Information.GetValueOrDefault("Basic Chart Path"));
                Charts[1] = new Ma2(location + Information.GetValueOrDefault("Basic Chart Path"));
            if (!Information["Advanced"].Equals("") &&
                File.Exists(location + Information.GetValueOrDefault("Advanced Chart Path")))
                Charts[2] = new Ma2(location + Information.GetValueOrDefault("Advanced Chart Path"));
            if (!Information["Expert"].Equals("") &&
                File.Exists(location + Information.GetValueOrDefault("Expert Chart Path")))
                Charts[3] = new Ma2(location + Information.GetValueOrDefault("Expert Chart Path"));
            if (!Information["Master"].Equals("") &&
                File.Exists(location + Information.GetValueOrDefault("Master Chart Path")))
                Charts[4] = new Ma2(location + Information.GetValueOrDefault("Master Chart Path"));
            if (!Information["Remaster"].Equals("") &&
                File.Exists(location + Information.GetValueOrDefault("Remaster Chart Path")))
                Charts[5] = new Ma2(location + Information.GetValueOrDefault("Remaster Chart Path"));
            if (!Information["Utage"].Equals("") &&
                File.Exists(location + Information.GetValueOrDefault("Utage Chart Path")))
                Charts[6] = new Ma2(location + Information.GetValueOrDefault("Utage Chart Path"));
        }

        Result = Compose();
        //Console.WriteLine(result);
    }

    /// <summary>
    ///     Construct compiler of a single song.
    /// </summary>
    /// <param name="location">Folder</param>
    /// <param name="targetLocation">Output folder</param>
    /// <param name="forUtage">True if for utage</param>
    public SimaiCompiler(bool strictDecimal, string location, string targetLocation, bool forUtage)
    {
        StrictDecimalLevel = strictDecimal;
        var ma2files = Directory.GetFiles(location, "*.ma2");
        Charts = new List<Chart>();
        MusicXML = new XmlInformation(location);
        Information = MusicXML.Information;
        var rotate = false;
        var rotateParameter = "";
        foreach (var pair in RotateDictionary)
            if (MusicXML.TrackID.Equals(pair.Key))
            {
                rotateParameter = pair.Value;
                rotate = true;
            }

        foreach (var ma2file in ma2files)
        {
            var chartCandidate = new Ma2(ma2file);
            if (rotate)
            {
                bool rotateParameterIsValid = Enum.TryParse(rotateParameter, out FlipMethod rotateParameterEnum);
                if (rotateParameterIsValid)
                {
                    chartCandidate.RotateNotes((rotateParameterEnum));
                }
                else
                {
                    throw new Exception("The given rotation method is invalid. Given: " + rotateParameter);
                }
            }
            Charts.Add(chartCandidate);
        }

        var ma2List = new List<string>();
        ma2List.AddRange(ma2files);

        Result = Compose(true, ma2List);
        //Console.WriteLine(result);
    }

    /// <summary>
    ///     Empty constructor.
    /// </summary>
    public SimaiCompiler()
    {
        StrictDecimalLevel = false;
        for (var i = 0; i < 7; i++) Charts.Add(new Simai());
        Charts = new List<Chart>();
        Information = new Dictionary<string, string>();
        MusicXML = new XmlInformation();
        Result = "";
    }

    public void WriteOut(string targetLocation, bool overwrite)
    {
        var sw = new StreamWriter(targetLocation + GlobalSep + "maidata.txt", !overwrite);
        {
            sw.WriteLine(Result);
        }
        sw.Close();
    }

    public override string Compose()
    {
        var result = "";
        // Console.WriteLine("StrictDecimal: "+StrictDecimalLevel);
        // Console.ReadKey();
        //Add Information
        {
            var beginning = "";
            beginning += $"&title={Information.GetValueOrDefault("Name")}{Information.GetValueOrDefault("SDDX Suffix")}\n";
            beginning += $"&wholebpm={Information.GetValueOrDefault("BPM")}\n";
            beginning += $"&artist={Information.GetValueOrDefault("Composer")}\n";
            beginning += $"&des={Information.GetValueOrDefault("Master Chart Maker")}\n";
            beginning += $"&shortid={Information.GetValueOrDefault("Music ID")}\n";
            beginning += $"&genre={Information.GetValueOrDefault("Genre")}\n";
            beginning += "&cabinet=";
            if (MusicXML.IsDXChart)
                beginning += "DX\n";
            else
                beginning += "SD\n";
            beginning += $"&version={MusicXML.TrackVersion}\n";
            beginning += "&ChartConverter=Neskol\n";
            beginning += "&ChartConvertTool=MaichartConverter\n";
            // string assemblyVersion = FileVersionInfo.GetVersionInfo(typeof(SimaiCompiler).Assembly.Location).ProductVersion ?? "Alpha Testing";
            // if (assemblyVersion.Contains('+')) assemblyVersion = assemblyVersion.Split('+')[0];
            // beginning += "&ChartConvertToolVersion=" + assemblyVersion + "\n";
            beginning += $"&ChartConvertToolVersion={Assembly.GetExecutingAssembly().GetName().Version}\n";
            beginning += "&smsg=See https://github.com/Neskol/MaichartConverter for updates\n";
            beginning += "\n";

            if (Information.TryGetValue("Easy", out var easy) &&
                Information.TryGetValue("Easy Chart Maker", out var easyMaker))
            {
                string difficultyCandidate = easy;
                if (StrictDecimalLevel && Information.TryGetValue("Easy Decimal", out var decimalLevel))
                {
                    difficultyCandidate = decimalLevel;
                }
                beginning += "&lv_1=" + difficultyCandidate + "\n";
                beginning += "&des_1=" + easyMaker + "\n";
                beginning += "\n";
            }

            if (Information.TryGetValue("Basic", out var basic) &&
                Information.TryGetValue("Basic Chart Maker", out var basicMaker))
            {
                string difficultyCandidate = basic;
                if (StrictDecimalLevel && Information.TryGetValue("Basic Decimal", out var decimalLevel))
                {
                    difficultyCandidate = decimalLevel;
                }
                beginning += "&lv_2=" + difficultyCandidate + "\n";
                beginning += "&des_2=" + basicMaker + "\n";
                beginning += "\n";
            }


            if (Information.TryGetValue("Advanced", out var advance) &&
                Information.TryGetValue("Advanced Chart Maker", out var advanceMaker))
            {
                string difficultyCandidate = advance;
                if (StrictDecimalLevel && Information.TryGetValue("Advanced Decimal", out var decimalLevel))
                {
                    difficultyCandidate = decimalLevel;
                }
                beginning += "&lv_3=" + difficultyCandidate + "\n";
                beginning += "&des_3=" + advanceMaker + "\n";
                beginning += "\n";
            }


            if (Information.TryGetValue("Expert", out var expert) &&
                Information.TryGetValue("Expert Chart Maker", out var expertMaker))
            {
                string difficultyCandidate = expert;
                if (StrictDecimalLevel && Information.TryGetValue("Expert Decimal", out var decimalLevel))
                {
                    difficultyCandidate = decimalLevel;
                }
                beginning += "&lv_4=" + difficultyCandidate + "\n";
                beginning += "&des_4=" + expertMaker + "\n";
                beginning += "\n";
            }


            if (Information.TryGetValue("Master", out var master) &&
                Information.TryGetValue("Master Chart Maker", out var masterMaker))
            {
                string difficultyCandidate = master;
                if (StrictDecimalLevel && Information.TryGetValue("Master Decimal", out var decimalLevel))
                {
                    difficultyCandidate = decimalLevel;
                }
                beginning += "&lv_5=" + difficultyCandidate + "\n";
                beginning += "&des_5=" + masterMaker + "\n";
                beginning += "\n";
            }


            if (Information.TryGetValue("Remaster", out var remaster) &&
                Information.TryGetValue("Remaster Chart Maker", out var remasterMaker))
            {
                string difficultyCandidate = remaster;
                if (StrictDecimalLevel && Information.TryGetValue("Remaster Decimal", out var decimalLevel))
                {
                    difficultyCandidate = decimalLevel;
                }
                beginning += "&lv_6=" + difficultyCandidate + "\n";
                beginning += "&des_6=" + remasterMaker;
                beginning += "\n";
                beginning += "\n";
            }

            result += beginning;
        }
        Console.WriteLine("Finished writing header of " + Information.GetValueOrDefault("Name"));

        //Compose Charts
        {
            for (var i = 0; i < Charts.Count; i++)
            {
                // Console.WriteLine("Processing chart: " + i);
                if (Charts[i] != null && !Information[Difficulty[i]].Equals(""))
                {
                    var isDxChart = Information.GetValueOrDefault("SDDX Suffix");
                    if (!Charts[i].IsDxChart) isDxChart = "";
                    result += "&inote_" + (i + 1) + "=\n";
                    result += Compose(Charts[i]);
                    CompiledChart.Add(Information.GetValueOrDefault("Name") + isDxChart + " [" + Difficulty[i] + "]");
                }

                result += "\n";
            }
        }
        Console.WriteLine("Finished composing.");
        return result;
    }

    /// <summary>
    ///     Return compose of specified chart.
    /// </summary>
    /// <param name="chart">Chart to compose</param>
    /// <returns>Maidata of specified chart WITHOUT headers</returns>
    public override string Compose(Chart chart)
    {
        return new Simai(chart).Compose();
    }

    /// <summary>
    ///     Compose utage Charts
    /// </summary>
    /// <param name="isUtage">switch to produce utage</param>
    /// <returns>Corresponding utage chart</returns>
    public override string Compose(bool isUtage, List<string> ma2files)
    {
        var result = "";
        //Add Information

        var beginning = "";
        beginning += "&title=" + Information.GetValueOrDefault("Name") + "[宴]" + "\n";
        beginning += "&wholebpm=" + Information.GetValueOrDefault("BPM") + "\n";
        beginning += "&artist=" + Information.GetValueOrDefault("Composer") + "\n";
        beginning += "&des=" + Information.GetValueOrDefault("Master Chart Maker") + "\n";
        beginning += "&shortid=" + Information.GetValueOrDefault("Music ID") + "\n";
        beginning += "&genre=" + Information.GetValueOrDefault("Genre") + "\n";
        beginning += "&cabinet=";
        if (MusicXML.IsDXChart)
            beginning += "DX\n";
        else
            beginning += "SD\n";
        beginning += "&version=" + MusicXML.TrackVersion + "\n";
        beginning += "&ChartConverter=Neskol\n";
        beginning += "&ChartConvertTool=MaichartConverter\n";
        string assemblyVersion = FileVersionInfo.GetVersionInfo(typeof(SimaiCompiler).Assembly.Location).ProductVersion ?? "Alpha Testing";
        if (assemblyVersion.Contains('+')) assemblyVersion = assemblyVersion.Split('+')[0];
        beginning += "&ChartConvertToolVersion=" + assemblyVersion + "\n"; beginning += "&smsg=See https://github.com/Neskol/MaichartConverter for updates\n";
        beginning += "\n";

        var defaultChartIndex = 7;
        if (ma2files.Count > 1)
        {
            defaultChartIndex = 2;
            foreach (var ma2file in ma2files)
            {
                string difficultyCandidate = Information["Utage"].Equals("") ? "宴" : $"{Information["Utage"]}?";
                if (StrictDecimalLevel && Information.TryGetValue("Utage Decimal", out var decimalLevel))
                {
                    difficultyCandidate = $"{decimalLevel}?";
                }
                beginning += $"&lv_{defaultChartIndex}={difficultyCandidate}\n\n";
                defaultChartIndex++;
            }

            defaultChartIndex = 0;
        }
        else
        {
            string difficultyCandidate = Information["Utage"].Equals("") ? "宴" : $"{Information["Utage"]}?";
            if (StrictDecimalLevel && Information.TryGetValue("Utage Decimal", out var decimalLevel))
            {
                difficultyCandidate = $"{decimalLevel}?";
            }
            beginning += $"&lv_{defaultChartIndex}={difficultyCandidate}\n\n";
        }


        result += beginning;
        Console.WriteLine("Finished writing header of " + Information.GetValueOrDefault("Name"));

        //Compose Charts

        if (defaultChartIndex < 7)
        {
            for (var i = 0; i < Charts.Count; i++)
            {
                // Console.WriteLine("Processing chart: " + i);
                var isDxChart = "Utage";
                result += "&inote_" + (i + 2) + "=\n";
                result += Compose(Charts[i]);
                CompiledChart.Add(Information.GetValueOrDefault("Name") + isDxChart + " [宴]");
                result += "\n";
            }
        }
        else
        {
            result += "&inote_7=\n";
            result += Compose(Charts[0]);
            CompiledChart.Add(Information.GetValueOrDefault("Name") + "Utage" + " [宴]");
        }

        Console.WriteLine("Finished composing.");
        return result;
    }
}
