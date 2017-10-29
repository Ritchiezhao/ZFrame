using UnityEngine;
using UnityEditor;


namespace ZFEditor
{
    public class UILine : UINode
    {
        /* Point
         * 端点定义 */
        public class Point
        {
            enum PointType
            {
                FixedPos,
                Anchor,
                Mouse
            }

            PointType mType = PointType.FixedPos;

            // for fixed pos
            Vector2 mFixedPos;

            // for anchor
            UINode mNode;
            float mPercentX;
            float mPercentY;


            public Point(UINode node, float percentX, float percentY)
            {
                Set_Anchor(node, percentX, percentY);
            }

            public void Set_FixedPos(Vector2 pos)
            {
                mType = PointType.FixedPos;
                mFixedPos = pos;
            }

            public void Set_Anchor(UINode node, float percentX, float percentY)
            {
                mType = PointType.Anchor;
                mNode = node;
                mPercentX = percentX;
                mPercentY = percentY;
            }

            public void Set_FollowMouse()
            {
                mType = PointType.Mouse;
            }

            public Vector2 GetPos()
            {
                if (mType == PointType.FixedPos)
                {
                    return mFixedPos;
                }
                else if (mType == PointType.Anchor)
                {
                    if (mNode != null)
                    {
                        return new Vector2(mNode.WorldArea.x + mNode.WorldArea.width * mPercentX, mNode.WorldArea.y + mNode.WorldArea.height * mPercentY);
                    }
                    else
                        return Vector2.zero;
                }
                else if (mType == PointType.Mouse)
                {
                    // todo: get mouse point //
                    return Vector2.zero;
                }
                return Vector2.zero;
            }
        }


        enum LineType
        {
            StraightLine,
            Bezier,
        }

        LineType mLineType;
        Point mBegin, mEnd;
        Color mColor = Color.white;

        // for bezier
        Vector3 mBezierTangent1, mBezierTangent2;

        public void SetColor(Color color)
        {
            mColor = color;
        }

        public void SetBeginPoint(Point pt)
        {
            mBegin = pt;
        }

        public void SetEndPoint(Point pt)
        {
            mEnd = pt;
        }

        public void SetStraight()
        {
            mLineType = LineType.StraightLine;
        }

        public void SetBezier(Vector2 tangent1, Vector2 tangent2)
        {
            mLineType = LineType.Bezier;
            mBezierTangent1 = tangent1.normalized;
            mBezierTangent2 = tangent2.normalized;
        }

        public override void Draw(Rect clipArea)
        {
            base.Draw(clipArea);

            Vector3 pos1 = mBegin.GetPos() - clipArea.position;
            Vector3 pos2 = mEnd.GetPos() - clipArea.position;

            if (mLineType == LineType.StraightLine)
            {
                Handles.color = mColor;
                Handles.DrawLine(pos1, pos2);
            }
            else if (mLineType == LineType.Bezier)
            {
                float len = (pos2 - pos1).magnitude * 0.4f;
                Handles.DrawBezier(pos1, pos2, pos1 + mBezierTangent1 * len, pos2 + mBezierTangent2 * len, mColor, null, 2.0f);
            }
        }
    }
}