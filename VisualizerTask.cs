using Avalonia;
using Avalonia.Input;
using Avalonia.Media;
using Manipulation.UI;
using System;
using System.Globalization;
using System.Linq;

namespace Manipulation;

public static class VisualizerTask
{
    public static double X = 220;
    public static double Y = -100;
    public static double Alpha = 0.05;
    public static double Wrist = 2 * Math.PI / 3;
    public static double Elbow = 3 * Math.PI / 4;
    public static double Shoulder = Math.PI / 2;

    public static Brush UnreachableAreaBrush = new SolidColorBrush(Avalonia.Media.Color.FromArgb(255, 255, 230, 230));
    public static Brush ReachableAreaBrush = new SolidColorBrush(Avalonia.Media.Color.FromArgb(255, 230, 255, 230));
    public static Pen ManipulatorPen = new Pen(Brushes.Black, 3);
    public static Brush JointBrush = new SolidColorBrush(Colors.Gray);

    public static void KeyDown(Visual visual, KeyEventArgs key)
    {
        // TODO: Добавьте реакцию на QAWS и пересчитывать Wrist
        if (key.Key == Key.W)
        {
            Elbow += Math.PI / 180;
        }

        if (key.Key == Key.A)
        {
            Shoulder -= Math.PI / 180;
        }

        if (key.Key == Key.S)
        {
            Elbow -= Math.PI / 180;
        }

        if (key.Key == Key.Q)
        {
            Shoulder += Math.PI / 180;
        }

        Wrist = -Alpha - Shoulder - Elbow;

        visual.InvalidateVisual(); // вызывает перерисовку канваса
    }

    public static void MouseMove(Visual visual, PointerEventArgs e)
    {
        // TODO: Измените X и Y пересчитав координаты (e.X, e.Y) в логические.

        var position = e.GetPosition(visual);
        double mouseX = position.X;
        double mouseY = position.Y;

        // Преобразование координат из окна в логические координаты
        Avalonia.Point mathPoint = ConvertWindowToMath(new Avalonia.Point(mouseX, mouseY), GetShoulderPos(visual));

        X = mathPoint.X;
        Y = mathPoint.Y;
        UpdateManipulator();
        visual.InvalidateVisual();
    }

    public static void MouseWheel(Visual visual, PointerWheelEventArgs e)
    {
        //Изменение по оси Y для обновления Alpha
        Alpha += e.Delta.Y;
        UpdateManipulator();
        visual.InvalidateVisual();
    }

    public static void UpdateManipulator()
    {
        double[] angles = ManipulatorTask.MoveManipulatorTo(X, Y, Alpha);
        if (angles.Any(double.IsNaN))
            return;
        Shoulder = angles[0];
        Elbow = angles[1];
        Wrist = angles[2];
        // Вызовите ManipulatorTask.MoveManipulatorTo и обновите значения полей Shoulder, Elbow и Wrist, 
        // если они не NaN. Это понадобится для последней задачи.
    }

    public static void DrawManipulator(DrawingContext context, Avalonia.Point shoulderPosition)
    {
        var joints = AnglesToCoordinatesTask.GetJointPositions(Shoulder, Elbow, Wrist);
        var points = new Avalonia.Point[joints.Length + 1];
        points[0] = shoulderPosition;

        var formattedText = new FormattedText(
            $"X={X:0}, Y={Y:0}, Alpha={Alpha:0.00}",
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            new Typeface("Arial"),
            12,
            Brushes.DarkRed);

        context.DrawText(formattedText, new Avalonia.Point(10, 10));

        DrawReachableZone(context, ReachableAreaBrush, UnreachableAreaBrush, shoulderPosition, joints);

        for (var i = 0; i < joints.Length; i++)
        {
            points[i + 1] = ConvertMathToWindow(joints[i], shoulderPosition);
            context.DrawLine(ManipulatorPen, points[i], points[i + 1]);
            context.DrawEllipse(JointBrush, null, points[i], 5, 5);
        }
    }

    private static void DrawReachableZone(
        DrawingContext context,
        Brush reachableBrush,
        Brush unreachableBrush,
        Avalonia.Point shoulderPos,
        Avalonia.Point[] joints)
    {
        var rmin = Math.Abs(Manipulator.UpperArm - Manipulator.Forearm);
        var rmax = Manipulator.UpperArm + Manipulator.Forearm;
        var mathCenter = new Avalonia.Point(joints[2].X - joints[1].X, joints[2].Y - joints[1].Y);
        var windowCenter = ConvertMathToWindow(mathCenter, shoulderPos);
        context.DrawEllipse(reachableBrush, null, windowCenter, rmax, rmax);
        context.DrawEllipse(unreachableBrush, null, windowCenter, rmin, rmin);
    }

    public static Avalonia.Point GetShoulderPos(Visual visual)
    {
        return new Avalonia.Point(visual.Bounds.Width / 2, visual.Bounds.Height / 2);
    }

    public static Avalonia.Point ConvertMathToWindow(Avalonia.Point mathPoint, Avalonia.Point shoulderPos)
    {
        return new Avalonia.Point(mathPoint.X + shoulderPos.X, shoulderPos.Y - mathPoint.Y);
    }

    public static Avalonia.Point ConvertWindowToMath(Avalonia.Point windowPoint, Avalonia.Point shoulderPos)
    {
        return new Avalonia.Point(windowPoint.X - shoulderPos.X, shoulderPos.Y - windowPoint.Y);
    }

    internal static void MouseWheel(Frame frame, PointerWheelEventArgs ev, object alpha)
    {
        throw new NotImplementedException();
    }
}
