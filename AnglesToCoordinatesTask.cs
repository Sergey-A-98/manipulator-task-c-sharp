using System;
using Avalonia;
using NUnit.Framework;
using static Manipulation.Manipulator;

namespace Manipulation
{
    public static class AnglesToCoordinatesTask
    {
        public static Point[] GetJointPositions(double shoulder, double elbow, double wrist)
        {
            var upperArm = Manipulator.UpperArm;
            var forearm = Manipulator.Forearm;
            var palm = Manipulator.Palm;

            var elbowX = upperArm * (float)Math.Cos(shoulder);
            var elbowY = upperArm * (float)Math.Sin(shoulder);
            var elbowPos = new Point((float)elbowX, (float)elbowY);

            var wristX = forearm * (float)Math.Cos(shoulder + Math.PI + elbow) + elbowX;
            var wristY = forearm * (float)Math.Sin(shoulder + Math.PI + elbow) + elbowY;
            var wristPos = new Point((float)wristX, (float)wristY);

            var palmX = palm * (float)Math.Cos(shoulder + Math.PI + elbow + Math.PI + wrist) + wristX;
            var palmY = palm * (float)Math.Sin(shoulder + Math.PI + elbow + Math.PI + wrist) + wristY;
            var palmEndPos = new Point((float)palmX, (float)palmY);
            return new Point[]
            {
                elbowPos,
                wristPos,
                palmEndPos
            };
        }
    }

    [TestFixture]
    public class AnglesToCoordinatesTask_Tests
    {
        [TestCase(Math.PI / 2, Math.PI / 2, Math.PI, Manipulator.Forearm + Manipulator.Palm, Manipulator.UpperArm)]
        [TestCase(Math.PI / 2, Math.PI / 2, Math.PI / 2, Manipulator.Forearm, Manipulator.UpperArm - Manipulator.Palm)]
        [TestCase(Math.PI / 2, 3 * Math.PI / 2, 3 * Math.PI / 2, -(Manipulator.Forearm),
            Manipulator.UpperArm - Manipulator.Palm)]
        [TestCase(Math.PI / 2, Math.PI, 3 * Math.PI, 0, Manipulator.Forearm + Manipulator.UpperArm + Manipulator.Palm)]
        public void TestGetJointPositions(double shoulder, double elbow, double wrist, double palmEndX, double palmEndY)
        {
            var joints = AnglesToCoordinatesTask.GetJointPositions(shoulder, elbow, wrist);
            double tolerance = 1e-5;
            Assert.AreEqual(palmEndX, joints[2].X, tolerance, "palm endX");
            Assert.AreEqual(palmEndY, joints[2].Y, tolerance, "palm endY");
            Assert.AreEqual(GetDistance(joints[0], new Point(0, 0)), UpperArm, tolerance);
            Assert.AreEqual(GetDistance(joints[0], joints[1]), Forearm, tolerance);
            Assert.AreEqual(GetDistance(joints[1], joints[2]), Palm, tolerance);
        }

        public double GetDistance(Point point1, Point point2)
        {
            var variableX = (point1.X - point2.X) * (point1.X - point2.X);
            var variableY = (point1.Y - point2.Y) * (point1.Y - point2.Y);
            return Math.Sqrt(variableX + variableY);
        }
    }
}