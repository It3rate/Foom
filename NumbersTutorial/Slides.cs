namespace MathDemo;

using MathDemo.Controls;
using Numbers;
using Numbers.Agent;
using Numbers.Drawing;
using Numbers.Mappers;
using Numbers.Renderer;
using NumbersCore.CoreConcepts.Temperature;
using NumbersCore.Primitives;
using NumbersSkia.Mappers;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;

public class Slides : DemoBase
{
    private Brain Brain { get; }
    private Random rnd = new Random();
    private SKPaint _oldTextPen;
    private SKPaint _newTextPen;
    public Slides(Brain brain)
    {
        Brain = brain;
        SKWorkspaceMapper.DefaultWorkspaceGhostText = CorePens.GetText(SKColor.Parse("#B0C0D0"), 18);
        SKWorkspaceMapper.DefaultWorkspaceText = CorePens.GetText(SKColor.Parse("#3030A0"), 18);
        _testIndex = 0;// 24;
        Pages.AddRange(new PageCreator[]
        {
            AnimationTest,
            RandomVsOrder_A,
            RandomVsOrder_B,
            GradientLine_A,
            Uncertainty_A,
            Categories_A,
            ValidMath_A,
            Selection_A,
            Selection_B,
            Polarity_A,
            InvertedBasis_A,

            ComparisonsBasis_A, // 10
            ComparisonsBasis_B,
            ComparisonsBasis_C,
            Ticks_A,
            SefDef_A,
            SegDef_B,
            //Nil_A,
            AddSubtract_A,
            AddSubtract_B,
            AddSubtract_C,
            MultiplyDivide_A,

            MultiplyDivide_B, // 20
            MultiplyDivide_C,
            MultiplyDivide_D,
            Bool_A,
            //Bool_Test,
            BoolCompare_A,
            //QualifiedBools_A,
            Joins_A, // 25
            Area_A,
            Area_B,
        });
    }

    private SKWorkspaceMapper AnimationTest()
    {
        var wm = new SKWorkspaceMapper(_currentMouseAgent, 100, 300, 1050, 350);
        wm.ShowAll();
        wm.DefaultShowNumbersOffset = true;

        var hd = wm.GetOrCreateDomainMapper(Domain.CreateDomain("Animation", 4, -10f, 20f, 0));

        var maskedNum = new MaskedNumber(Polarity.Aligned, true, 8, 12, 16, 22);
        hd.Domain.AddNumber(maskedNum, true);
        var mnm = new SkMaskedNumberMapper(_currentMouseAgent, maskedNum);
        hd.AddNumberMapper(mnm);

        //var left = hd.CreateNumberFromFloats(0, 2, true);
        //var right = hd.CreateNumberFromFloats(0, 3, true);
        //Transform transform = Brain.AddTransform(left.Number, right.Number, OperationKind.Multiply);
        //transform.Repeats = hd.CreateNumberFromFloats(0, 1.5f).Number;
        //var tm = wm.GetOrCreateTransformMapper(transform);
        //hd.Domain.AddNumber(transform.Result);

        return wm;
    }

    private SKWorkspaceMapper RandomVsOrder_A()
    {
        var wm = new SKWorkspaceMapper(_currentMouseAgent, 500, 300, 550, 350);
        wm.ShowNone();
        string[] txt = new string[] {
         "All things are ordered or random. Random is unknowable, ordered is knowable.",
         "Randomness happens when something is supplying information you can't predict, or you are supplying a guess for something unpredictable.",
         "All random things happen within a range, all ordered things happen along a range.",
           };
        wm.CreateTextMapper(txt, new SKSegment(50, 50, 100, 50));

        wm.CreateImageMapper("0_ants.png", new SKSegment(50, 230, 450, 230));
        DrawCircles(wm, -250, 250, -150, 150);
        return wm;
    }
    private SKWorkspaceMapper RandomVsOrder_B()
    {
        var wm = RandomVsOrder_A();
        wm.DefaultShowPolarity = false;
        wm.DefaultShowValueMarkers = false;
        wm.AppendText(
         "All predictable things become unknown (random) beyond a certain resolution.",
        "Many things have order we can't distinguish: unseen patterns, or differences to small or large to measure.",
        "Perceptible does not mean predictable. Ordered sequences maximize both perceptibility and predictability."
        );
        var im = wm.LastImageMapper();
        im.Reset("1_antsSingleFile.png");

        var hd0 = wm.GetOrCreateDomainMapper(Domain.CreateDomain("RandomVsOrder", 1, 250), wm.BottomSegment.Inverted());
        var num0 = hd0.CreateNumberFromFloats(250, 250f);
        var hd1 = wm.GetOrCreateDomainMapper(Domain.CreateDomain("RandomVsOrder", 1, 150), wm.RightSegment.Inverted());
        var num1 = hd1.CreateNumberFromFloats(150, 150f);

        num0.OnChanged += (sender, e) =>
        {
            DrawCircles(wm, (int)-num0.Number.StartValue, (int)num0.Number.EndValue, (int)-num1.Number.StartValue, (int)num1.Number.EndValue);
        };
        num1.OnChanged += (sender, e) =>
        {
            DrawCircles(wm, (int)-num0.Number.StartValue, (int)num0.Number.EndValue, (int)-num1.Number.StartValue, (int)num1.Number.EndValue);
        };

        var paint = CorePens.GetPen(SKColors.Red, 12);
        var w = 22;
        for (int i = 0; i < 25; i++)
        {
            var path = wm.CreatePathMapper();
            path.Pen = CorePens.GetPen(SKColor.FromHsl(i * 4f + 150, 70, 50), 8); ;
            var x = 500 + i * w;
            var y = 700;
            path.SetOval(new SKPoint(x, y), new SKPoint(x + w, y + w));
        }
        return wm;
    }
    private SKWorkspaceMapper GradientLine_A()
    {
        var wm = new SKWorkspaceMapper(_currentMouseAgent, 100, 350, 800, 400);
        wm.ShowNone();
        string[] txt = new string[] {
            "Order can be represented by a gradient line - this is a proxy.",
            "Traits are measurable categories, things like size, position, color, sound, mood, status.",
            "We can cut paths through these traits, and represent them on a line."
           };
        wm.CreateTextMapper(txt, new SKSegment(50, 50, 100, 50));

        wm.CreateImageMapper("3_path.jpg", new SKSegment(80, 150, 530, 150));


        wm.DefaultShowGradientNumberLine = false;
        wm.DefaultShowPolarity = false;
        wm.DefaultShowPolarity = false;
        wm.DefaultShowFractions = false;
        wm.DefaultShowMinorTicks = false;
        wm.DefaultShowTicks = false;

        // thermometer
        var left = 678;
        var right = 805;
        var top = 135;
        wm.CreateImageMapper("therm.png", new SKSegment(left, top, right, top));
        var mid = (right - left) / 2f + left;

        var guideline = new SKSegment(mid + 2, top + 116, mid + 2, top + 425);
        var hd = wm.GetOrCreateDomainMapper(Domain.CreateDomain("therm", 1, 50), guideline);
        var num = hd.CreateNumber(new Focal(50, -10));
        num.InvertPolarity();

        // stars
        var w = 80;
        var hue = 100;
        var x = 970;
        var y = 50;
        var pts = 3;
        var count = 8;
        var paths = new List<SKPathMapper>();
        var val = num.Number.EndValue * 2;
        byte midH = 128;
        byte green = 30;
        for (int i = 0; i < count; i++)
        {
            var path = wm.CreatePathMapper();
            path.DefaultBrush = CorePens.GetBrush(new SKColor((byte)(midH + val), green, (byte)(midH - val)));
            path.Pen = CorePens.GetPen(SKColor.FromHsl(hue, 80, 20), 5);
            path.SetStar(new SKPoint(x, y), new SKPoint(x + w, y + w), pts++);
            y += 100;
            hue += 150 / count;
            paths.Add(path);
        }

        num.OnChanged += (sender, e) =>
        {
            val = Math.Min(100, Math.Max(-100, num.Number.EndValue * 2));
            var brush = CorePens.GetBrush(new SKColor((byte)(midH + val), green, (byte)(midH - val)));
            foreach (var path in paths)
            {
                path.DefaultBrush = brush;
            }
        };

        return wm;
    }
    private SKWorkspaceMapper Uncertainty_A()
    {
        var wm = new SKWorkspaceMapper(_currentMouseAgent, 200, 650, 800, 300);
        wm.ShowNone();
        wm.DefaultShowTicks = true;
        string[] txt = new string[] {
            "We partition these paths into the highest resolution we can (or we care about), and accept things below this resolution will be unpredictable.",
            "We bound the range of unpredictability into something useful for the context, and stop there.",
            "All measures have uncertain, unimportant, or unpredictable aspects, but we can quantify and qualify these unknowns."
           };
        wm.CreateTextMapper(txt, new SKSegment(50, 50, 100, 50));

        var control = new ShapeControl(_currentMouseAgent, 175, 320, 600, 400);
        wm.AddUIControl(control);

        var dm = wm.CreateLinkedNumber(control.Fill.Hue);
        wm.CreateLinkedNumber(control.Radius);
        wm.CreateLinkedNumber(control.RadiusOffset);
        wm.CreateLinkedNumber(control.Points);

        return wm;
    }
    private SKWorkspaceMapper Categories_A()
    {
        var wm = new SKWorkspaceMapper(_currentMouseAgent, 200, 650, 800, 300);
        wm.ShowNone();
        wm.DefaultShowTicks = true;
        wm.DefaultShowFractions = true;
        string[] txt = new string[] {
            "When measurable properties cluster, we can create categories.",
            "These categories can be as strict or loose as needed, and we don't need to consider all properties at once.",
           };
        wm.CreateTextMapper(txt, new SKSegment(50, 50, 100, 50));

        var control = new FruitControl(_currentMouseAgent, 150, 150, 900, 300);
        wm.AddUIControl(control);

        wm.CreateLinkedNumber(control.Fill.Hue);
        wm.CreateLinkedNumber(control.Fill.Lightness);
        wm.CreateLinkedNumber(control.Size);
        wm.DefaultShowMinorTicks = true;
        wm.CreateLinkedNumber(control.AspectRatio);
        var (_, conv) = wm.CreateLinkedNumber(control.Convexity);
        //conv.OnSelected += (sender, e) =>
        //{
        //    Trace.WriteLine(control.GetValuesAsString());
        //};

        var label = wm.CreateTextMapper(new string[] { "Random" }, new SKSegment(60, 180, 100, 180));

        var fruitCount = FruitControl.FruitData.Count;
        var guideline = new SKSegment(70, 600, 70, 200);
        var kindDomain = wm.GetOrCreateDomainMapper(Domain.CreateDomain("kind", 1, 0, fruitCount, 0), guideline);
        var kindNum = kindDomain.CreateNumber(new Focal(0, 1));
        kindNum.OnChanged += (sender, e) =>
        {
            var name = control.UpdateToIndex((int)-kindNum.Number.StartValue);
            label.Lines[0] = name;
        };
        return wm;
    }
    private SKWorkspaceMapper ValidMath_A()
    {
        var wm = new SKWorkspaceMapper(_currentMouseAgent, 100, 350, 800, 400);
        wm.ShowNone();
        wm.DefaultShowTicks = true;
        wm.DefaultShowFractions = true;
        string[] txt = new string[] {
            "Gradients can be any shape, including circles. If they are anything more than a single path, they become multidimensional.",
            "*** Everything that can happen on these gradient lines is valid math. ***",
            "Many of these 'valid things' are intuitive, useful, and not covered by existing math. Some will follow here, but there are many more.",
            "You should explore this!",
           };
        wm.CreateTextMapper(txt, new SKSegment(50, 50, 100, 50));

        wm.CreateImageMapper("4_parkPath.jpg", new SKSegment(350, 250, 850, 250));

        var path = " 705.00,329.00, 706.00,340.00, 710.00,350.00, 723.00,365.00, 732.00,389.00, 731.00,396.00, 724.00,400.00, 715.00,401.00, 714.00,403.00, 709.00,403.00, 705.00,395.00, 685.00,374.00, 675.00,370.00, 644.00,383.00, 608.00,405.00, 593.00,407.00, 563.00,407.00, 560.00,400.00, 546.00,382.00, 532.00,335.00, 502.00,321.00, 500.00,318.00, 489.00,315.00, 467.00,314.00, 456.00,317.00, 412.00,343.00, 406.00,350.00, 397.00,364.00, 397.00,384.00, 400.00,395.00, 400.00,433.00, 398.00,436.00, 397.00,458.00, 400.00,466.00, 402.00,481.00, 402.00,514.00, 410.00,523.00, 425.00,530.00, 437.00,532.00, 478.00,532.00, 484.00,537.00, 484.00,559.00, 482.00,567.00, 470.00,584.00, 460.00,588.00, 454.00,594.00, 449.00,612.00, 449.00,619.00, 463.00,634.00, 471.00,636.00, 512.00,637.00, 516.00,636.00, 520.00,631.00, 527.00,629.00, 538.00,629.00, 557.00,625.00, 558.00,622.00, 570.00,613.00, 584.00,605.00, 606.00,597.00, 626.00,595.00, 638.00,603.00, 641.00,608.00, 647.00,612.00, 663.00,635.00, 665.00,641.00, 664.00,668.00, 666.00,670.00, 680.00,669.00, 683.00,625.00, 684.00,570.00, 682.00,554.00, 682.00,510.00, 679.00,466.00, 693.00,453.00, 704.00,447.00, 716.00,436.00, 735.00,437.00, 743.00,435.00, 747.00,416.00, 746.00,408.00, 744.00,406.00, 744.00,391.00, 749.00,379.00, 762.00,364.00, 770.00,350.00, 776.00,333.00, 776.00,331.00, 771.00,328.00, 742.00,327.00, 733.00,320.00";
        var pm = wm.CreatePathMapper();
        pm.SetStoredPoints(path);
        pm.Pen = CorePens.GetPen(SKColors.DarkBlue, 10);

        var guideline = new SKSegment(100, 200, 1100, 200);
        var hd = wm.GetOrCreateDomainMapper(Domain.CreateDomain("validMath", 128, 0, 1, 0), guideline);
        var num = hd.CreateNumber(new Focal(0, 5));
        pm.SetPartialPath(-num.Number.StartTValue(), num.Number.EndTValue(), true);

        num.OnChanged += (sender, e) =>
        {
            pm.SetPartialPath(-num.Number.StartTValue(), num.Number.EndTValue(), true);
        };
        return wm;
    }
    private SKWorkspaceMapper Selection_A()
    {
        var wm = new SKWorkspaceMapper(_currentMouseAgent, 100, 350, 800, 400);
        wm.ShowAll();
        wm.DefaultShowPolarity = false;
        wm.DefaultShowFractions = false;
        wm.DefaultShowMinorTicks = false;
        string[] txt = new string[] {
            "There are two possible initial actions: create a selection, or choose an existing one.",
        "Create a selection by starting with a new position and expanding it.",
        "While stretching this tiny selection, new material is appended to the selection. Both positive and negative work.",
           };
        wm.CreateTextMapper(txt, new SKSegment(50, 50, 100, 50));

        var guideline = new SKSegment(200, 400, 1000, 400);
        var hd = wm.GetOrCreateDomainMapper(Domain.CreateDomain("Selection", 1, 10), guideline);
        return wm;
    }
    private SKWorkspaceMapper Selection_B()
    {
        var wm = Selection_A();
        wm.AppendText(
        "If the final selection is less than the resolution, it still has position and length, just the length is to small to quantify.",
        "'Zero' length is not nil, zero position is also not nil. You can not destroy a selection by adjusting it."

        //"You can create a segment by selecting a position and expanding in the positive or negative direction.",
        //"You can also select an existing segment.",
        //"All selections have a start and end point, and therefore a direction.",
        //"Selection with '0' length, means the length is below the smallest resolution, and you can't tell the direction."
           );
        var last = wm.LastDomainMapper();
        return wm;
    }
    private SKWorkspaceMapper Nil_A()
    {
        var wm = new SKWorkspaceMapper(_currentMouseAgent, 600 - 480, 400, 480 * 2, 400);
        wm.ShowAll();
        wm.DefaultShowMinorTicks = true;
        wm.DefaultDomainTicks = 2;
        wm.DefaultDomainRange = 12;
        string[] txt = new string[] {
        "Segments can not be removed by stretching them. That would be deletion, and the most you can do here is shrink.",
        "They can be made to have a length too small to measure, which is zero, but not nil.",
        "Zero length segments can still be re-expanded. There are no operations that can be applied to nil (no selection).",
        "Division by this zero is a valid operation. The result is a value at least larger than the dividend, and potentially larger than the maximum value.",
        "What does moving from the zero position imply? Do you think our number 5 represents a point or a segment here?",
        };
        wm.CreateTextMapper(txt, new SKSegment(50, 50, 100, 50));
        var hd0 = wm.GetOrCreateDomainMapper(Domain.CreateDomain("MultiplyDivide", wm.DefaultDomainTicks, wm.DefaultDomainRange));
        return wm;
    }
    private SKWorkspaceMapper Polarity_A()
    {
        var wm = new SKWorkspaceMapper(_currentMouseAgent, 100, 350, 800, 400);
        wm.ShowAll();
        string[] txt = new string[] {
        "These gradient lines also have two directions, positive red or positive blue. This is the polarity.",
        "You can make multiple segments on a gradient line, and they can have different polarities.",
        "In the real world, these polarities have opposite meanings. If one direction is happy, the other might be sad.",
           };
        wm.CreateTextMapper(txt, new SKSegment(50, 50, 100, 50));

        wm.CreateImageMapper("polarity_hs.jpg", new SKSegment(600 - 170, 420, 600 + 170, 420));

        var guideline = new SKSegment(600 - 450, 280, 600 + 450, 280);
        var hd = wm.GetOrCreateDomainMapper(Domain.CreateDomain("Polarity", 1, 10), guideline);
        return wm;
    }
    private SKWorkspaceMapper InvertedBasis_A()
    {
        var wm = new SKWorkspaceMapper(_currentMouseAgent, 100, 350, 900, 400);
        wm.ShowAll();
        wm.DefaultShowPolarity = true;
        string[] txt = new string[] {
            "What about inverted numbers? Every basis has an inverted basis, and inverted numbers use it instead of the regular basis.",
            "It always has the same length, it always defines zero and one, and it always points in the *opposite* direction.",
            "A number line can contain both types of numbers. ",
           };
        wm.CreateTextMapper(txt, new SKSegment(50, 50, 100, 50));

        wm.CreateImageMapper("eastWest.png", new SKSegment(600 - 170, 470, 600 + 170, 470));


        var guideline = new SKSegment(600 - 450, 280, 600 + 450, 280);

        var hd = wm.GetOrCreateDomainMapper(Domain.CreateDomain("Selection", 4, 3), guideline);
        var nm0 = hd.CreateNumberFromFloats(0, 2f);


        return wm;
    }

    private SKWorkspaceMapper ComparisonsBasis_A()
    {
        var wm = new SKWorkspaceMapper(_currentMouseAgent, 100, 350, 900, 400);
        wm.ShowAll();
        wm.DefaultShowNumbersOffset = true;
        wm.DefaultShowPolarity = false;
        string[] txt = new string[] {
         //"Traits can be combined into new categories. Area, color, shortcuts, and speed are examples of combined traits.",
        "With multiple selections, you can compare lengths. The first is the basis of the comparison.",
        "You can say things like longer, to the left of, twice as long, overlapping."
           };
        wm.CreateTextMapper(txt, new SKSegment(50, 50, 100, 50));

        wm.CreateImageMapper("cats.png", new SKSegment(600 - 170, 420, 600 + 170, 420));

        var guideline = new SKSegment(600 - 450, 280, 600 + 450, 280);
        var hd = wm.GetOrCreateDomainMapper(Domain.CreateDomain("Selection", 4, 3), guideline);
        hd.startOffset = 12;
        hd.CreateNumberFromFloats(0, 3f);
        hd.CreateNumberFromFloats(0, 1.5f);
        return wm;
    }
    private SKWorkspaceMapper ComparisonsBasis_B()
    {
        var wm = ComparisonsBasis_A();
        wm.AppendText(
            "Switching basis is the reciprocal, shorter, to the right of, half as long."
        );
        var ld = wm.LastDomainMapper();
        ld.RemoveNumberMapperByIndex(2);

        return wm;
    }
    private SKWorkspaceMapper ComparisonsBasis_C()
    {
        var wm = ComparisonsBasis_B();
        wm.AppendText(
            "Using a different basis for the same property is a form of conversion."
        );

        wm.LastImageMapper().SetBitmap("CF.jpg");

        var ld = wm.LastDomainMapper();
        wm.RemoveDomainMapper(ld);

        wm.ShowAll();
        wm.DefaultShowMinorTicks = false;
        wm.DefaultShowPolarity = false;
        wm.DefaultShowFractions = false;
        wm.OffsetNextLineBy(-70);
        var dmC = wm.GetOrCreateDomainMapper(TemperatureDomain.GetCelsiusDomain(), null, null, "Celsius");
        var dmF = wm.GetOrCreateDomainMapper(TemperatureDomain.GetFahrenheitDomain(), null, null, "Fahrenheit");
        var nm0 = dmF.CreateNumberFromFloats(35f, 8f);
        dmC.LinkNumber(nm0.Number);


        return wm;
    }
    private SKWorkspaceMapper Ticks_A()
    {
        var wm = new SKWorkspaceMapper(_currentMouseAgent, 100, 350, 900, 400);
        wm.ShowAll();
        string[] txt = new string[] {
        "It is handy to break the units into 'ticks'. Math works without this, but there are difficulties. Our brains use ticks.",
        "If the unit is your smallest possible measurement, it is inaccurate by definition, and scaling to 1M will have error.",
        "Easier to have larger measure and divide it accurately, which is what ticks are.",
        "Ticks allow conversion between precision by narrowing numbers, otherwise you must renumber everything.",
        "TrueNegative ticks are rounding. Zero sized ticks are crazy, and that is what we use in real numbers.",
           };
        wm.CreateTextMapper(txt, new SKSegment(50, 50, 100, 50));
        wm.LineOffsetSize = 60;

        var hd0 = wm.GetOrCreateDomainMapper(Domain.CreateDomain("Ticks", 1, 6));
        hd0.Label = "Whole Values";
        var hd1 = wm.GetOrCreateDomainMapper(Domain.CreateDomain("Ticks", 8, 6));
        hd1.Label = "Higher Resolution";
        var hd2 = wm.GetOrCreateDomainMapper(Domain.CreateDomain("Ticks", 5, 20));
        hd2.Label = "Inverted Resolution";
        hd2.Domain.BasisIsReciprocal = true;
        //hd2.ShowMinorTicks = false;
        hd2.ShowTickMarkerValues = true;
        return wm;
    }
    private SKWorkspaceMapper SefDef_A()
    {
        var wm = new SKWorkspaceMapper(_currentMouseAgent, 100, 50, 1050, 800);
        wm.ShowNone();
        wm.DefaultDrawPen = CorePens.GetPen(SKColor.Parse("#6090D0"), 8);
        string[] txt = new string[] {
            "If we call a segment 3+8, we just get 11. How can we say 'from 3 to 8'?",
            "Complex numbers face this same dilemma.",
            "Look close at use cases. From and to. Above and below zero. Deep and high. Vertical axis direction. CCW..."
           };
        wm.CreateTextMapper(txt, new SKSegment(50, 50, 100, 50));
        wm.CreateImageMapper("clock.png", new SKSegment(120, 280, 450, 280));

        var guideline = new SKSegment(120 + 170, 280 + 170, 460, 280 + 170);
        var hd = wm.GetOrCreateDomainMapper(Domain.CreateDomain("clock", 60, 0, 12, 0), guideline);
        var nm = hd.CreateNumberFromFloats(0, 11f);
        wm.ShowAll();

        return wm;
    }
    private SKWorkspaceMapper SegDef_B()
    {
        var wm = SefDef_A();
        wm.ShowAll();
        wm.DefaultShowNumbersOffset = true;
        wm.AppendText(
            "The start point uses the inverted basis. The end point uses the polarity's basis.",
            "Remember these are two zero based values joined together. Like 'from 9 deep to 3 high'.",
            "It is handy to write the end point in the polarity's units (3i+2, or -3-2i)."
        );
        wm.DefaultShowPolarity = true;
        wm.CreateImageMapper("iceberg.png", new SKSegment(550, 150, 1050, 150));

        var guideline = new SKSegment(1100, 850, 1100, 50);
        var hd0 = wm.GetOrCreateDomainMapper(Domain.CreateDomain("UnitUnot", 4, -11, 5, 0), guideline);
        return wm;
    }
    private SKWorkspaceMapper AddSubtract_A()
    {
        var wm = new SKWorkspaceMapper(_currentMouseAgent, 600 - 450, 300, 900, 300);
        wm.ShowAll();
        wm.DefaultShowPolarity = false;
        wm.DefaultShowMinorTicks = false;
        wm.DefaultShowFractions = false;
        wm.DefaultDomainTicks = 1;
        wm.DefaultDomainRange = 16;
        string[] txt = new string[] {
            "All adjustments to a segments must be continuous (because gradients are continuous), so all changes are akin to stretching.",
            "To stretch something, imagine taking a section of the gradient and stretching it. ",
            "All selected elements will stretch with the expanding or contracting gradient.",
            "You can select any section of the gradient, but there are two special types of selection.",
           };
        wm.CreateTextMapper(txt, new SKSegment(50, 50, 100, 50));

        var hd0 = wm.GetOrCreateDomainMapper(Domain.CreateDomain("AddSubtract", wm.DefaultDomainTicks, wm.DefaultDomainRange));
        return wm;
    }
    private SKWorkspaceMapper AddSubtract_B()
    {
        var wm = AddSubtract_A();
        wm.AppendText(
            "If you select the basis unit of the number, stretching it will result in multiplication (or division)."
        );

        var leftDm = wm.LastDomainMapper();
        leftDm.Label = "  A";
        var rightDm = wm.GetOrCreateDomainMapper(Domain.CreateDomain("AddSubtract", wm.DefaultDomainTicks, wm.DefaultDomainRange));
        rightDm.Label = "* B";
        var resultDm = wm.GetOrCreateDomainMapper(Domain.CreateDomain("AddSubtract", wm.DefaultDomainTicks, wm.DefaultDomainRange));
        resultDm.Label = "= C";
        resultDm.ShowNumbersOffset = true;

        rightDm.BasisNumber.Focal = leftDm.BasisNumber.Focal;
        resultDm.BasisNumber.Focal = leftDm.BasisNumber.Focal;
        var leftNum = leftDm.CreateNumberFromFloats(0, 2);
        var rightNum = rightDm.CreateNumberFromFloats(0, 3);
        Transform transform = Brain.AddTransform(leftNum.Number, rightNum.Number, OperationKind.Multiply);
        var tm = wm.GetOrCreateTransformMapper(transform);
        tm.Do2DRender = false;
        var resultNum = resultDm.Domain.AddNumber(transform.Result);
        return wm;
    }
    private SKWorkspaceMapper AddSubtract_C()
    {
        var wm = AddSubtract_B();
        wm.DefaultShowPolarity = true;
        wm.AppendText(
            "If you select an area that is exactly the same as a number segment, stretching it will result in addition (or subtraction)."
        //"What does moving from the zero position imply? Do you think our number 5 represents a point or a segment here?"
        );
        wm.LastDomainMapper().ShowPolarity = true;


        var last = wm.LastDomainMapper();
        var guideline = last.Guideline.ShiftOffLine(100);
        var leftDm = wm.GetOrCreateDomainMapper(Domain.CreateDomain("MultiplyDivide", wm.DefaultDomainTicks, wm.DefaultDomainRange), guideline);
        leftDm.Label = "  A";
        leftDm.ShowNumbersOffset = true;

        guideline = leftDm.Guideline.ShiftOffLine(60);
        var rightDm = wm.GetOrCreateDomainMapper(Domain.CreateDomain("MultiplyDivide", wm.DefaultDomainTicks, wm.DefaultDomainRange), guideline);
        rightDm.Label = "+ B";
        rightDm.ShowNumbersOffset = true;
        leftDm.AlignDomainTo(rightDm);

        guideline = guideline.ShiftOffLine(60);
        var resultDm = wm.GetOrCreateDomainMapper(Domain.CreateDomain("MultiplyDivide", wm.DefaultDomainTicks, wm.DefaultDomainRange), guideline);
        resultDm.Label = "= C";
        resultDm.ShowNumbersOffset = true;


        rightDm.BasisNumber.Focal = leftDm.BasisNumber.Focal;
        resultDm.BasisNumber.Focal = leftDm.BasisNumber.Focal;
        var leftNum = leftDm.CreateNumberFromFloats(0, 2);
        var rightNum = rightDm.CreateNumberFromFloats(0, 3);
        Transform transform = Brain.AddTransform(leftNum.Number, rightNum.Number, OperationKind.Add);
        var tm = wm.GetOrCreateTransformMapper(transform);
        tm.Do2DRender = false;
        var resultNum = resultDm.Domain.AddNumber(transform.Result);

        return wm;
    }
    private SKWorkspaceMapper MultiplyDivide_A()
    {
        var wm = new SKWorkspaceMapper(_currentMouseAgent, 600 - 480, 300, 420 * 2, 300);
        wm.ShowAll();
        wm.DefaultDomainTicks = 4;
        wm.DefaultDomainRange = 3;
        wm.DefaultShowMinorTicks = false;
        wm.DefaultShowNumbersOffset = true;
        wm.LineOffsetSize = 50;
        string[] txt = new string[] {
            "Inverted numbers use the inverted basis. They stretch as normal, and they also invert the polarity.",
            "Repeated multiplying by an inverted number forms a cycle.",
            "Repeating operations on a selection is what powers are.",
            "Repetition can apply to multiplication, addition, bool operations, and any combinations of these, including powers.",
           };
        wm.CreateTextMapper(txt, new SKSegment(50, 50, 100, 50));

        var hd0 = wm.GetOrCreateDomainMapper(Domain.CreateDomain("MultiplyDivide", wm.DefaultDomainTicks, wm.DefaultDomainRange));
        hd0.BasisNumberMapper.Guideline.FlipAroundStartPoint();
        hd0.Label = "Inverted Basis Position";
        var hd1 = wm.GetOrCreateDomainMapper(Domain.CreateDomain("MultiplyDivide", wm.DefaultDomainTicks, wm.DefaultDomainRange));
        hd1.Label = "Inverted Unit (i)";
        var iValue = hd1.CreateInvertedNumberFromFloats(0, 1);
        var hd2 = wm.GetOrCreateDomainMapper(Domain.CreateDomain("MultiplyDivide", wm.DefaultDomainTicks, wm.DefaultDomainRange));
        var oneValue = hd2.CreateNumberFromFloats(0, 1);
        hd2.Label = "Current Value";
        return wm;

    }

    private SKWorkspaceMapper MultiplyDivide_B()
    {
        var wm = new SKWorkspaceMapper(_currentMouseAgent, 600 - 480, 250, 480 * 2, 250);
        wm.ShowAll();
        wm.DefaultDomainTicks = 30;
        wm.DefaultDomainRange = 3;
        wm.DefaultShowMinorTicks = false;
        wm.DefaultShowNumbersOffset = true;
        string[] txt = new string[] {
            "Repeated multiplying can converge on Euler's number.",
            "Accounting for direction is the beginnings of Sin and Cos.",
            //"Repeated addition using partial numbers can create the Fibonacci sequence.",
           };
        wm.CreateTextMapper(txt, new SKSegment(50, 50, 100, 50));

        var hd = wm.GetOrCreateDomainMapper(Domain.CreateDomain("MultiplyDivide", wm.DefaultDomainTicks, wm.DefaultDomainRange));
        var n0 = hd.CreateNumberFromFloats(0, 1, true);
        var rightNm = hd.CreateNumberFromFloats(0, 1, true);
        var result = rightNm.Number;
        for (int i = 0; i < wm.DefaultDomainTicks + 1; i++) // same rows as ticks to allow Euler's number
        {
            Transform transform = Brain.AddTransform(result, n0.Number, OperationKind.Multiply);
            var tm = wm.GetOrCreateTransformMapper(transform);
            result = hd.Domain.AddNumber(transform.Result);
        }

        return wm;
    }
    private SKWorkspaceMapper MultiplyDivide_C()
    {
        var wm = new SKWorkspaceMapper(_currentMouseAgent, 600 - 480, 250, 480 * 2, 250);
        wm.ShowAll();
        wm.DefaultDomainTicks = 4;
        wm.DefaultDomainRange = 8;
        wm.DefaultShowMinorTicks = false;
        wm.DefaultShowNumbersOffset = false;
        string[] txt = new string[] {
            "Addition can repeat just like multiplication. The result can be separated or combined.",
           };
        wm.CreateTextMapper(txt, new SKSegment(50, 50, 100, 50));

        var hdOrg = wm.GetOrCreateDomainMapper(Domain.CreateDomain("MultiplyDivide", 4, -1, 16, 0));
        var hd = wm.GetOrCreateDomainMapper(Domain.CreateDomain("MultiplyDivide", 4, -1, 16, 0));
        hd.ShowTicks = true;
        hd.ShowMinorTicks = true;
        hd.ShowTickMarkerValues = true;
        var n0 = hdOrg.CreateNumberFromFloats(-1f, 1f, true);
        var n1 = hd.CreateNumberFromFloats(0, .75f, true);
        var result = n1.Number;
        for (int i = 0; i < 10; i++) // same rows as ticks to allow Euler's number
        {
            Transform transform = Brain.AddTransform(result, n0.Number, OperationKind.Add);
            var tm = wm.GetOrCreateTransformMapper(transform);
            result = hd.Domain.AddNumber(transform.Result);
        }

        return wm;
    }
    private SKWorkspaceMapper MultiplyDivide_D()
    {
        var wm = new SKWorkspaceMapper(_currentMouseAgent, 600 - 480, 250, 480 * 2, 250);
        wm.ShowAll();
        wm.DefaultDomainTicks = 1;
        wm.DefaultDomainRange = 22;
        wm.DefaultShowMinorTicks = false;
        wm.DefaultShowNumbersOffset = true;
        string[] txt = new string[] {
            "Repeated addition using partial numbers can create the Fibonacci sequence.",
            "This is convert to the previous number's polarity and add it.",
            "The odd and even separation will also happen with this.",
           };
        wm.CreateTextMapper(txt, new SKSegment(50, 50, 100, 50));

        var hd = wm.GetOrCreateDomainMapper(Domain.CreateDomain("MultiplyDivide", wm.DefaultDomainTicks, wm.DefaultDomainRange));
        hd.CreateNumberFromFloats(0, 1, true);
        hd.CreateInvertedNumberFromFloats(0, 1, true);
        hd.CreateNumberFromFloats(1, 2, true);
        hd.CreateInvertedNumberFromFloats(2, 3, true);
        hd.CreateNumberFromFloats(3, 5, true);
        hd.CreateInvertedNumberFromFloats(5, 8, true);
        hd.CreateNumberFromFloats(8, 13, true);
        hd.CreateInvertedNumberFromFloats(13, 21, true);
        hd.CreateNumberFromFloats(21, 34, true);

        return wm;
    }



    private SKWorkspaceMapper Bool_A()
    {
        var wm = new SKWorkspaceMapper(_currentMouseAgent, 420, 170, 600, 700);
        wm.ShowNone();
        wm.DefaultDomainTicks = 1;
        wm.DefaultDomainRange = 64;
        wm.DefaultShowPolarity = true;
        //wm.DefaultShowBasis = true;
        wm.DefaultShowBasisMarkers = false;
        wm.DefaultShowGradientNumberLine = true;
        //wm.DefaultShowTicks = true;
        //wm.DefaultShowMinorTicks = false;

        string[] txt = new string[] {
        "Direct On/Off comparisons are probably the most obvious thing you can do with segments. With two values there are 4 possible configurations.",
        "There are 16 possible outputs given two inputs. Many of these map to language, words like and, or, not, except, more, never.",
        "Results can be used to conditionally activate branch paths. They can also can act 'physically' on segments, like stop on collide.",
           };
        wm.CreateTextMapper(txt, new SKSegment(50, 50, 100, 50));

        wm.CreateImageMapper("boolOps.png", new SKSegment(40, 150, 380, 150));

        var hd = wm.GetOrCreateDomainMapper(Domain.CreateDomain("Bool", wm.DefaultDomainTicks, wm.DefaultDomainRange));
        hd.ShowNumbersOffset = true;
        var n0 = hd.CreateNumberFromFloats(32, 16, true);
        var n1 = hd.CreateNumberFromFloats(8, 8, true);
        wm.OffsetNextLineBy(30);
        List<Transform> transforms = new List<Transform>();
        List<SKPathMapper> paths = new List<SKPathMapper>();

        //var kind = OperationKind.NAND;
        foreach (var kind in OperationKindExtension.BoolOpKinds())
        {
            var bdm = hd.Duplicate();
            bdm.Label = "       " + kind.GetName();
            Transform transform = Brain.AddTransform(n0.Number, n1.Number, kind);
            wm.GetOrCreateTransformMapper(transform);
            bdm.Domain.AddNumber(transform.Result);
            transforms.Add(transform);
            var pt = bdm.Guideline.EndPoint + new SKPoint(10, -8);
            paths.Add(AddCircle(wm, pt, 4, 0));
        }

        n0.OnChanged += (sender, e) => { UpdateSignals(transforms, paths); };
        n1.OnChanged += (sender, e) => { UpdateSignals(transforms, paths); };
        UpdateSignals(transforms, paths);

        return wm;
    }
    private SKWorkspaceMapper Bool_Test()
    {
        var wm = new SKWorkspaceMapper(_currentMouseAgent, 410, 170, 600, 700);
        wm.ShowNone();
        wm.DefaultDomainTicks = 1;
        wm.DefaultDomainRange = 64;
        wm.DefaultShowPolarity = true;
        wm.DefaultShowBasisMarkers = false;
        wm.DefaultShowGradientNumberLine = true;

        string[] txt = new string[] {
        "How should segments going in opposite directions compare? How about inverted segments?",
        "it feels like these should all give different answers. The interpretation for operations (like multiply) is how B causes A to change.",
        "So these can be though of as a bool comparison along the segment, and leaving, removing, inverting, and/or negating that section of A.",
           };
        wm.CreateTextMapper(txt, new SKSegment(50, 50, 100, 50));

        wm.CreateImageMapper("boolOpsInvA.png", new SKSegment(50, 160, 350, 160));

        var hd = wm.GetOrCreateDomainMapper(Domain.CreateDomain("Bool_Test", wm.DefaultDomainTicks, wm.DefaultDomainRange));
        hd.Label = "A & B";
        hd.ShowNumbersOffset = true;
        var n0 = hd.CreateNumberFromFloats(40, 0, true);
        var n1 = hd.CreateNumberFromFloats(20, 20, true);
        hd.CreateNumberFromFloats(999, -999, true);
        Transform transform = Brain.AddTransform(n0.Number, n1.Number, OperationKind.AND);
        wm.GetOrCreateTransformMapper(transform);
        hd.Domain.AddNumber(transform.Result);

        wm.OffsetNextLineBy(100);
        hd = wm.GetOrCreateDomainMapper(Domain.CreateDomain("Bool_Test2", wm.DefaultDomainTicks, wm.DefaultDomainRange));
        hd.Label = "A & B, opposite direction";
        hd.ShowNumbersOffset = true;
        n0 = hd.CreateNumberFromFloats(40, 0, true);
        n1 = hd.CreateNumberFromFloats(-20, -20, true);
        hd.CreateNumberFromFloats(999, -999, true);
        hd.CreateNumberFromFloats(0, -20, true);

        wm.OffsetNextLineBy(100);
        hd = wm.GetOrCreateDomainMapper(Domain.CreateDomain("Bool_Test2", wm.DefaultDomainTicks, wm.DefaultDomainRange));
        hd.Label = "A & B, opposite direction";
        hd.ShowNumbersOffset = true;
        n1 = hd.CreateNumberFromFloats(-20, -20, true);
        n0 = hd.CreateNumberFromFloats(40, 0, true);
        hd.CreateNumberFromFloats(999, -999, true);
        hd.CreateNumberFromFloats(0, -20, true);

        wm.OffsetNextLineBy(100);
        hd = wm.GetOrCreateDomainMapper(Domain.CreateDomain("Bool_Test2", wm.DefaultDomainTicks, wm.DefaultDomainRange));
        hd.Label = "A & B, inverted polarity";
        hd.ShowNumbersOffset = true;
        n0 = hd.CreateNumberFromFloats(40, 0, true);
        n1 = hd.CreateNumberFromFloats(20, 20, true);
        n1.InvertPolarity();
        hd.CreateNumberFromFloats(999, -999, true);
        var nr = hd.CreateNumberFromFloats(20, 0, true);
        nr.InvertPolarity();

        wm.OffsetNextLineBy(100);
        hd = wm.GetOrCreateDomainMapper(Domain.CreateDomain("Bool_Test2", wm.DefaultDomainTicks, wm.DefaultDomainRange));
        hd.Label = "A & B, inverted polarity";
        hd.ShowNumbersOffset = true;
        n0 = hd.CreateNumberFromFloats(40, 0, true);
        n0.InvertPolarity();
        n1 = hd.CreateNumberFromFloats(20, 20, true);
        n1.InvertPolarity();
        hd.CreateNumberFromFloats(999, -999, true);
        nr = hd.CreateNumberFromFloats(20, 0, true);

        return wm;
    }
    private SKWorkspaceMapper BoolCompare_A()
    {
        var wm = new SKWorkspaceMapper(_currentMouseAgent, 270, 250, 600, 700);
        wm.ShowNone();
        wm.DefaultDomainTicks = 1;
        wm.DefaultDomainRange = 32;
        wm.DefaultShowPolarity = true;
        wm.DefaultShowBasisMarkers = false;
        wm.DefaultShowGradientNumberLine = true;

        string[] txt = new string[] {
            "Comparisons are also possible with segments. A is LessThan B means A is fully to the left of B.",
            "A is LessThanOrEqual means one segment is not to the right of another.",
            "LessThanAndEqual means there is overlap on the left of B.",
           };
        wm.CreateTextMapper(txt, new SKSegment(50, 50, 100, 50));


        var hd = wm.GetOrCreateDomainMapper(Domain.CreateDomain("BoolCompare", wm.DefaultDomainTicks, wm.DefaultDomainRange));
        hd.ShowNumbersOffset = true;
        var n0 = hd.CreateNumberFromFloats(16, 16, true);
        var n1 = hd.CreateNumberFromFloats(8, 8, true);
        wm.OffsetNextLineBy(30);
        List<Transform> transforms = new List<Transform>();
        List<SKPathMapper> paths = new List<SKPathMapper>();
        foreach (var kind in OperationKindExtension.BoolCompareKinds())
        {
            var bdm = hd.Duplicate();
            bdm.Label = "     " + kind.GetName();
            Transform transform = Brain.AddTransform(n0.Number, n1.Number, kind);
            wm.GetOrCreateTransformMapper(transform);
            bdm.Domain.AddNumber(transform.Result);
            transforms.Add(transform);
            var pt = bdm.Guideline.EndPoint + new SKPoint(10, -8);
            paths.Add(AddCircle(wm, pt, 4, 0));
        }

        n0.OnChanged += (sender, e) => { UpdateSignals(transforms, paths); };
        n1.OnChanged += (sender, e) => { UpdateSignals(transforms, paths); };
        UpdateSignals(transforms, paths);
        return wm;
    }
    private SKWorkspaceMapper QualifiedBools_A()
    {
        var wm = new SKWorkspaceMapper(_currentMouseAgent, 100, 350, 800, 400);
        string[] txt = new string[] {
        "Numbers and merge results can be compared, and the results can be described with quantitative bools, like prepositions:",
        "near, over, to the left of, at least etc. These can all be constructed with segment definitions and bool operations.",
           };
        wm.CreateTextMapper(txt, new SKSegment(50, 50, 100, 50));
        return wm;
    }
    private SKWorkspaceMapper Joins_A()
    {
        var wm = new SKWorkspaceMapper(_currentMouseAgent, 100, 150, 800, 600);
        string[] txt = new string[] {
        "Join with branches, this happens as alternate paths on the same domain (same unit and trait).",
        "Can be an optional or required paths, triggered or blocked by bool results.",
        "Ornamental patterns and glyphs are examples of these visually. TYLOXP etc",
        "Predictions and plans are by definition alternate paths. Words like 'either', 'detour', 'try' suggest them.",
           };
        wm.CreateTextMapper(txt, new SKSegment(50, 50, 100, 50));


        return wm;
    }
    private SKWorkspaceMapper Area_A()
    {
        var wm = new SKWorkspaceMapper(_currentMouseAgent, 450, 20, 520, 660);
        wm.DefaultShowMinorTicks = false;
        string[] txt = new string[] {
        "Join domains (area), use the same traits (spatial, optical, tactile),",
            "but combine different aspects of them (xy, rgb, curved/smooth)",
        "Can be a 2D graph, join endpoints, and opposite tips.",
            "Drag out 4 lines to combine, triangles are alternate view.",
           };
        wm.CreateTextMapper(txt, new SKSegment(50, 50, 100, 50));

        var center = new SKPoint(780, 320);
        var w = 280;

        var guideline = new SKSegment(center.X - w, center.Y, center.X + w, center.Y);
        var hMapper = wm.GetOrCreateDomainMapper(Domain.CreateDomain("area", 100, 10), guideline);
        var hNum = hMapper.Domain.CreateNumberFromFloats(2, 9);

        guideline = new SKSegment(center.X, center.Y + w, center.X, center.Y - w);
        var vMapper = wm.GetOrCreateDomainMapper(Domain.CreateDomain("area", 100, 10), guideline);
        var vNum = vMapper.Domain.CreateNumberFromFloats(3, 6);


        Transform transform = Brain.AddTransform(hNum, vNum, OperationKind.Multiply);
        var tm = wm.GetOrCreateTransformMapper(transform);
        tm.Do2DRender = true;
        tm.EquationPoint = new SKPoint(hMapper.StartPoint.X - 220, vMapper.MidPoint.Y - 20);


        CreateSimilarDomain(hMapper.Domain, 1f, 20, hNum.Focal);
        CreateSimilarDomain(hMapper.Domain, 1.08f, 20, vNum.Focal);
        CreateSimilarDomain(hMapper.Domain, 1.2f, 100, transform.Result.Focal);



        return wm;
    }
    private SKWorkspaceMapper Area_B()
    {
        var wm = new SKWorkspaceMapper(_currentMouseAgent, 100, 350, 800, 400);
        string[] txt = new string[] {
        "Joint traits (mph), math is the same as with branches and other joins.",
        "Common strategy is to normalize one trait (miles per gallon, liters per 100km)",
        "We also create paths through complex joins, like dark or saturated colors, or a path through the woods.",
           };
        wm.CreateTextMapper(txt, new SKSegment(50, 50, 100, 50));
        return wm;
    }
    // branches
    // ornaments, letters
    // motions, collisions
    // mapping to language
    // parts of speech
    #region Utils
    private SKDomainMapper CreateLowResDomain(int rangeSize, float offset, params Focal[] focals)
    {
        var newDomain = Domain.CreateDomain("lowres", 4, rangeSize);
        foreach (var focal in focals)
        {
            newDomain.CreateNumber(focal);
        }
        var wm = _currentMouseAgent.WorkspaceMapper;
        var result = wm.AddDomain(newDomain, offset, true);
        result.ShowGradientNumberLine = true;
        result.ShowBasis = true;
        result.ShowBasisMarkers = true;
        result.ShowMinorTicks = true;
        result.ShowPolarity = true;
        return result;
    }
    private SKDomainMapper CreateSimilarDomain(Domain domain, float offset, int rangeSize, params Focal[] focals)
    {
        var newDomain = Domain.CreateDomain(domain.Trait.Name, (int)domain.BasisFocal.LengthInTicks, rangeSize);
        foreach (var focal in focals)
        {
            newDomain.CreateNumber(focal);
        }
        var result = _currentMouseAgent.WorkspaceMapper.AddDomain(newDomain, offset, true, -200);
        result.ShowBasis = true;
        result.ShowBasisMarkers = true;
        result.ShowMinorTicks = false;
        return result;
    }
    /// <summary>
    /// Create lines with focal pairs.
    /// </summary>
    private List<Domain> CreateDomainLines(MouseAgent mouseAgent, Trait trait, Focal basisFocal, params long[] focalPositions)
    {
        var result = new List<Domain>();
        var wm = mouseAgent.WorkspaceMapper;
        var padding = 1.4;
        long maxPos = (long)Math.Max((focalPositions.Max() * padding), basisFocal.AbsLengthInTicks * padding);
        long minPos = (long)Math.Min((focalPositions.Min() * padding), -basisFocal.AbsLengthInTicks * padding);
        var range = new Focal(minPos, maxPos);
        var rangeLen = (double)range.LengthInTicks;
        var yt = 0.1f;
        var ytStep = (float)(0.8 / Math.Floor(focalPositions.Length / 2.0));
        //var domain = trait.AddDomain(basisFocal, range);
        ////domain.BasisIsReciprocal = true;
        //result.Add(domain);
        for (int i = 1; i < focalPositions.Length; i += 2)
        {
            var focal = new Focal(focalPositions[i - 1], focalPositions[i]);

            var domain = trait.AddDomain(basisFocal.Clone(), focal, "demoDomain"); // clone to avoid duplicate focals.
                                                                                   //domain.BasisIsReciprocal = true;
            result.Add(domain);

            var num = domain.CreateNumber(focal);
            mouseAgent.Workspace.AddDomains(true, domain);
            var displaySeg = wm.GetHorizontalSegment(yt, 100);
            var y = displaySeg.StartPoint.Y;

            var sz = domain.BasisIsReciprocal ? 0.01f : 0.05f;
            var unitSeg = displaySeg.SegmentAlongLine(0.5f - sz, 0.5f + sz);
            //var unitSeg = new SKSegment((float)unitStart, y, (float)unitStart + 20f, y);
            var dm = wm.GetOrCreateDomainMapper(domain, displaySeg, unitSeg);
            dm.ShowGradientNumberLine = true;
            dm.ShowNumbersOffset = true;
            dm.ShowBasisMarkers = true;
            dm.ShowBasis = true;
            yt += ytStep;
        }

        return result;
    }
    private void DrawCircles(SKWorkspaceMapper wm, int minX, int maxX, int minY, int maxY)
    {
        var w = 20;
        wm.ClearPathMappers();
        var minXt = Math.Min(minX, maxX);
        var maxXt = Math.Max(minX, maxX);
        var minYt = Math.Min(minY, maxY);
        var maxYt = Math.Max(minY, maxY);
        for (int i = 0; i < 40; i++)
        {
            var path = wm.CreatePathMapper();
            path.Pen = CorePens.GetPen(SKColor.FromHsl((rnd.Next(100) + 150), 70, 50), 8);
            var x = (rnd.Next(minXt, maxXt)) + 750;
            var y = (rnd.Next(minYt, maxYt)) + 450;
            path.SetOval(new SKPoint(x, y), new SKPoint(x + w, y + w));
        }
    }
    private SKPathMapper AddCircle(SKWorkspaceMapper wm, SKPoint pt, int radius, int hue)
    {
        var path = wm.CreatePathMapper();
        path.Pen = CorePens.GetPen(SKColor.FromHsl(hue, 80, 60), 4);
        var x = pt.X + radius;
        var y = pt.Y + radius;
        path.SetOval(new SKPoint(x - radius, y - radius), new SKPoint(x + radius * 2, y + radius * 2));
        path.IsPermanent = true;
        return path;
    }
    private void UpdateSignals(List<Transform> transforms, List<SKPathMapper> paths)
    {
        var red = 0;
        var yellow = 40;
        var green = 100;
        for (int i = 0; i < transforms.Count; i++)
        {
            transforms[i].Apply();
            var hue = transforms[i].IsFalse ? red : transforms[i].IsEqual ? green : yellow;
            paths[i].Pen.Color = SKColor.FromHsl(hue, 80, 60);
        }
    }
    #endregion
}
