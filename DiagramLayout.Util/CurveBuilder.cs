using System;
using System.Collections.Generic;
using System.Drawing;

namespace DiagramLayout.DrawingUtil
{
    public class CurveBuilder
    {
        private void pb_Bezier_Paint(object sender, Diagram e)
        {
            Point P1 = new Point(10, 300);
            Point P2 = new Point(180, 50);
            Point P3 = new Point(320, 300);

            ZeichneBezier(6, P1, P2, P3, e, true);
        }

        private void ZeichneBezier(int n, Point P1, Point P2, Point P3, Diagram d, bool initial)
        {
            if (initial)
            {
                //g.DrawLine(kpStift, P1, P2);
                //g.DrawLine(kpStift, P2, P3);
            }

            if (n > 0)
            {
                Point P12 = new Point((P1.X + P2.X) / 2, (P1.Y + P2.Y) / 2);
                Point P23 = new Point((P2.X + P3.X) / 2, (P2.Y + P3.Y) / 2);
                Point P123 = new Point((P12.X + P23.X) / 2, (P12.Y + P23.Y) / 2);

                ZeichneBezier(n - 1, P1, P12, P123, d, false);
                ZeichneBezier(n - 1, P123, P23, P3, d, false);
            }
            else
            {
                d.DrawLine(P1, P2);
                d.DrawLine(P2, P3);
            }
        }
    }

    public class Diagram
    {
        private List<DiagramObject> _diagramObjects = new List<DiagramObject>();

        public void DrawLine(Point p1, Point p2)
        {

        }
    }

    public class DiagramObject
    {

    }

}
