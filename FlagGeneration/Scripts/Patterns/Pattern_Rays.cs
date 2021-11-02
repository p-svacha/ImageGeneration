using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace FlagGeneration
{
    class Pattern_Rays : FlagMainPattern
    {
        // Attributes
        private ColorScheme ChosenColorScheme;

        // Ray Spacing
        private float AlternateAngleOffset;
        private const float ALTERNATE_ANGLE_OFFSET_MIN = 0.15f; // of anglestep
        private const float ALTERNATE_ANGLE_OFFSET_MAX = 0.4f; // of anglestep

        // Chances
        private const float CENTER_DOUBLE_RAY_CHANCE = 0.65f; // Chance to double rays when origin is center

        // Origin Overlay
        private const float RAY_ORIGIN_CIRCLE_CHANCE = 0.5f;
        private const float RAY_ORIGIN__INNER_CIRCLE_CHANCE = 0.5f;
        private const float RAY_ORIGIN_CIRCLE_UNIQUE_COLOR_CHANCE = 0.3f;
        private const float RAY_ORIGIN_CIRCLE_SIZE_MIN = FlagHeight * 0.07f;
        private const float RAY_ORIGIN_CIRCLE_SIZE_MAX = FlagHeight * 0.3f;


        private Dictionary<int, int> NumRays = new Dictionary<int, int>()
        {
            {3, 30},
            {4, 30},
            {5, 70},
            {6, 35},
            {7, 50},
            {8, 30},
            {9, 25},
            {10, 12},
            {11, 12},
            {12, 8},
            {13, 12},
            {14, 8},
            {15, 12},
            {16, 8},
        };

        private enum RayOrigin
        {
            BotLeftCorner,
            TopLeftCorner,
            BotRightCorner,
            TopRightCorner,
            LeftCenter,
            BotCenter,
            RightCenter,
            TopCenter,
            Center
        }
        private Dictionary<RayOrigin, int> RayOrigins = new Dictionary<RayOrigin, int>()
        {
            {RayOrigin.TopLeftCorner, 3 },
            {RayOrigin.BotLeftCorner, 5 },
            {RayOrigin.BotRightCorner, 3 },
            {RayOrigin.TopRightCorner, 3 },
            {RayOrigin.LeftCenter, 3 },
            {RayOrigin.BotCenter, 3 },
            {RayOrigin.RightCenter, 3 },
            {RayOrigin.TopCenter, 3 },
            {RayOrigin.Center, 10 },
        };

        private enum ColorScheme
        {
            Alternating,
            Alternating3,
            Alternating4,
            Symmetrical,
            Unique,
        }
        private Dictionary<ColorScheme, int> ColorSchemes = new Dictionary<ColorScheme, int>()
        {
            {ColorScheme.Alternating, 50 },
            {ColorScheme.Alternating3, 25 },
            {ColorScheme.Alternating4, 15 },
            {ColorScheme.Symmetrical, 30 },
            {ColorScheme.Unique, 15 },
        };

        private enum RaySpacing
        {
            Even,
            Alternating,
        }
        private Dictionary<RaySpacing, int> RaySpacings = new Dictionary<RaySpacing, int>()
        {
            {RaySpacing.Even, 10 },
            {RaySpacing.Alternating, 6 }
        };

        public override void DoApply()
        {
            int numRays = GetWeightedRandomInt(NumRays);
            RayOrigin rayOrigin = GetWeightedRandomEnum<RayOrigin>(RayOrigins);
            if (rayOrigin == RayOrigin.Center && R.NextDouble() < CENTER_DOUBLE_RAY_CHANCE) numRays *= 2;
            if (rayOrigin == RayOrigin.Center && (numRays == 11 || numRays == 13)) numRays = 16; // High prime number of rays is not allowed when origin is center

            RaySpacing raySpacing = GetWeightedRandomEnum<RaySpacing>(RaySpacings);
            if (numRays % 2 != 0) raySpacing = RaySpacing.Even;
            AlternateAngleOffset = RandomRange(ALTERNATE_ANGLE_OFFSET_MIN, ALTERNATE_ANGLE_OFFSET_MAX);

            List<Color> rayColors = GetRayColors(numRays, rayOrigin);
            DrawRays(rayOrigin, rayColors, raySpacing);
            DrawOriginCircle(rayOrigin);
        }

        /// <summary>
        /// Draws the rays on the flag according to the given modifiers
        /// </summary>
        private void DrawRays(RayOrigin rayOrigin, List<Color> rayColors, RaySpacing raySpacing)
        {
            if (rayOrigin == RayOrigin.BotLeftCorner || rayOrigin == RayOrigin.BotRightCorner || rayOrigin == RayOrigin.TopLeftCorner || rayOrigin == RayOrigin.TopRightCorner) DrawCornerRays(rayOrigin, rayColors, raySpacing);
            else if(rayOrigin == RayOrigin.BotCenter || rayOrigin == RayOrigin.RightCenter || rayOrigin == RayOrigin.TopCenter || rayOrigin == RayOrigin.LeftCenter) DrawSideRays(rayOrigin, rayColors, raySpacing);
            else if(rayOrigin == RayOrigin.Center) DrawCenterRays(rayOrigin, rayColors, raySpacing);
        }

        /// <summary>
        /// Handles and draws the ray origin overlays
        /// </summary>
        private void DrawOriginCircle(RayOrigin rayOrigin)
        {
            Vector2 origin = GetOriginPoint(rayOrigin);
            if(R.NextDouble() < RAY_ORIGIN_CIRCLE_CHANCE)
            {
                Color additionalRandomColor = ColorManager.GetRandomColor(FlagColors);
                Color circleColor = FlagColors[R.Next(0, FlagColors.Count)];
                if (ChosenColorScheme == ColorScheme.Unique) circleColor = additionalRandomColor; // If ray colors are unique, so is the overlay
                else if (R.NextDouble() < RAY_ORIGIN_CIRCLE_UNIQUE_COLOR_CHANCE) circleColor = additionalRandomColor;

                float circleRadius = RandomRange(RAY_ORIGIN_CIRCLE_SIZE_MIN, RAY_ORIGIN_CIRCLE_SIZE_MAX);
                if (rayOrigin == RayOrigin.BotLeftCorner || rayOrigin == RayOrigin.BotRightCorner || rayOrigin == RayOrigin.TopLeftCorner || rayOrigin == RayOrigin.TopRightCorner) circleRadius *= 2f;
                if (rayOrigin == RayOrigin.BotCenter || rayOrigin == RayOrigin.RightCenter || rayOrigin == RayOrigin.TopCenter || rayOrigin == RayOrigin.LeftCenter) circleRadius *= 1.5f;

                DrawCircle(Svg, origin, circleRadius, circleColor);

                if (R.NextDouble() < RAY_ORIGIN_CIRCLE_CHANCE)
                {
                    Color innerColor = ColorManager.GetSecondaryColor(circleColor, FlagColors);

                    float innerMaxSize = circleRadius * 0.95f; 
                    float innerMinSize = circleRadius * 0.5f;
                    float innerRadius = RandomRange(innerMinSize, innerMaxSize);

                    DrawCircle(Svg, origin, innerRadius, innerColor);
                }
            }
        }

        private void DrawCornerRays(RayOrigin rayOrigin, List<Color> rayColors, RaySpacing raySpacing)
        {
            int numRays = rayColors.Count;

            Vector2 originPoint = GetOriginPoint(rayOrigin);
            Vector2 nextPoint = Vector2.Zero;
            Vector2 lastPoint = Vector2.Zero;
            float targetHeight = 0f;
            bool inverted = false;

            switch (rayOrigin)
            {
                case RayOrigin.TopLeftCorner:
                    nextPoint = new Vector2(0f, FlagHeight);
                    lastPoint = new Vector2(FlagWidth, 0);
                    targetHeight = FlagHeight;
                    break;

                case RayOrigin.BotLeftCorner:
                    nextPoint = new Vector2(0, 0);
                    lastPoint = new Vector2(FlagWidth, FlagHeight);
                    targetHeight = 0;
                    break;

                case RayOrigin.BotRightCorner:
                    nextPoint = new Vector2(FlagWidth, 0);
                    lastPoint = new Vector2(0, FlagHeight);
                    targetHeight = 0;
                    inverted = true;
                    break;

                case RayOrigin.TopRightCorner:
                    nextPoint = new Vector2(FlagWidth, FlagHeight);
                    lastPoint = new Vector2(0, 0);
                    targetHeight = FlagHeight;
                    inverted = true;
                    break;
            }

            for (int i = 0; i < numRays; i++)
            {
                float angle = GetAngle(numRays, 90, i + 1, raySpacing);
                float b = (float)(FlagHeight * Math.Tan(DegToRad(angle)));
                Vector2 newPoint = new Vector2(inverted ? FlagWidth - b : b, targetHeight);
                if (i == numRays - 1) newPoint = lastPoint;
                List<Vector2> vertices = new List<Vector2>() { originPoint, nextPoint, newPoint };
                DrawPolygon(Svg, vertices.ToArray(), rayColors[i]);
                nextPoint = newPoint;
            }
        }

        private void DrawSideRays(RayOrigin rayOrigin, List<Color> rayColors, RaySpacing raySpacing)
        {
            int numRays = rayColors.Count;
            Vector2 origin = GetOriginPoint(rayOrigin);

            if (rayOrigin == RayOrigin.BotCenter || rayOrigin == RayOrigin.TopCenter)
            {
                Vector2 point2 = Vector2.Zero;
                Vector2 point3 = Vector2.Zero;
                float targetHeight = 0f;

                if(rayOrigin == RayOrigin.BotCenter)
                {
                    origin = new Vector2(FlagCenter.X, FlagHeight);
                    targetHeight = 0f;
                }
                if (rayOrigin == RayOrigin.TopCenter)
                {
                    origin = new Vector2(FlagCenter.X, 0);
                    targetHeight = FlagHeight;
                }

                for (int i = 0; i < numRays; i++)
                {
                    float angle = GetAngle(numRays, 180, i + 1, raySpacing);

                    if (i == 0) point2 = new Vector2(0, FlagHeight - targetHeight);
                    else point2 = point3;

                    if (angle < 90)
                    {
                        float b = (float)(Math.Tan(DegToRad(angle)) * FlagCenter.X);
                        point3 = new Vector2(0, rayOrigin == RayOrigin.BotCenter ? FlagHeight - b : b);
                    }
                    else if (angle > 90)
                    {
                        float b = (float)(Math.Tan(DegToRad(180 - angle)) * FlagCenter.X);
                        point3 = new Vector2(FlagWidth, rayOrigin == RayOrigin.BotCenter ? FlagHeight - b : b);
                    }
                    else
                    {
                        float offset = (float)(FlagHeight / Math.Tan(DegToRad(angle)));
                        point3 = new Vector2(origin.X + offset, targetHeight);
                    }

                    if (i == numRays - 1) point3 = new Vector2(FlagWidth, FlagHeight - targetHeight);
                    List<Vector2> vertices = new List<Vector2>() { origin, point2, point3 };

                    // Fix corners with 4 rays
                    if (numRays == 4 && i == 1) vertices.Insert(2, new Vector2(0, targetHeight));
                    if (numRays == 4 && i == 2) vertices.Insert(2, new Vector2(FlagWidth, targetHeight));

                    DrawPolygon(Svg, vertices.ToArray(), rayColors[i]);
                }
            }
            if (rayOrigin == RayOrigin.LeftCenter || rayOrigin == RayOrigin.RightCenter)
            {
                Vector2 point2 = Vector2.Zero;
                Vector2 point3 = Vector2.Zero;
                float targetWidth = 0f;

                if (rayOrigin == RayOrigin.LeftCenter)
                {
                    origin = new Vector2(0, FlagCenter.Y);
                    targetWidth = FlagWidth;
                }
                if (rayOrigin == RayOrigin.RightCenter)
                {
                    origin = new Vector2(FlagWidth, FlagCenter.Y);
                    targetWidth = 0;
                }

                for (int i = 0; i < numRays; i++)
                {
                    float angle = GetAngle(numRays, 180, i + 1, raySpacing);

                    if (i == 0) point2 = new Vector2(FlagWidth - targetWidth, 0);
                    else point2 = point3;

                    if (angle < 90)
                    {
                        float b = (float)(Math.Tan(DegToRad(angle)) * FlagCenter.Y);
                        point3 = new Vector2(rayOrigin == RayOrigin.RightCenter ? FlagWidth - b : b, 0);
                    }
                    else if (angle > 90)
                    {
                        float b = (float)(Math.Tan(DegToRad(180 - angle)) * FlagCenter.Y);
                        point3 = new Vector2(rayOrigin == RayOrigin.RightCenter ? FlagWidth - b : b, FlagHeight);
                    }
                    else
                    {
                        point3 = new Vector2(targetWidth, FlagCenter.Y);
                    }

                    if (i == numRays - 1) point3 = new Vector2(FlagWidth - targetWidth, FlagHeight);
                    List<Vector2> vertices = new List<Vector2>() { origin, point2, point3 };

                    // Fix corners with 3 rays
                    if (numRays == 3 && i == 1)
                    {
                        vertices.Insert(2, new Vector2(targetWidth, 0));
                        vertices.Insert(3, new Vector2(targetWidth, FlagHeight));
                    }
                    // Fix corners with 4/6/8 rays
                    else if((numRays == 4 || numRays == 6 || numRays == 8) && i == numRays / 2 - 1) vertices.Insert(2, new Vector2(targetWidth, 0));
                    else if((numRays == 4 || numRays == 6 || numRays == 8) && i == numRays / 2) vertices.Insert(2, new Vector2(targetWidth, FlagHeight));

                    DrawPolygon(Svg, vertices.ToArray(), rayColors[i]);
                }
            }
        }

        private void DrawCenterRays(RayOrigin rayOrigin, List<Color> rayColors, RaySpacing raySpacing)
        {
            int numRays = rayColors.Count;

            Vector2 originPoint = GetOriginPoint(rayOrigin);
            int offsetAngle = 90 * R.Next(0, 4);

            float radius = FlagWidth * 2;
            
            for (int i = 0; i < numRays; i++)
            {
                float startAngle = offsetAngle + GetAngle(numRays, 360, i, raySpacing);
                float endAngle = offsetAngle + GetAngle(numRays, 360, i + 1, raySpacing);
                //if (i == 0) startAngle = offsetAngle;
                //if (i == numRays - 1) endAngle = offsetAngle;
                float startX = (float)(Math.Sin(DegToRad(startAngle)) * radius);
                float startY = (float)(Math.Cos(DegToRad(startAngle)) * radius);
                float endX = (float)(Math.Sin(DegToRad(endAngle)) * radius);
                float endY = (float)(Math.Cos(DegToRad(endAngle)) * radius);
                Vector2 point2 = FlagCenter + new Vector2(startX, startY);
                Vector2 point3 = FlagCenter + new Vector2(endX, endY);
                List<Vector2> vertices = new List<Vector2>() { originPoint, point2, point3 };
                DrawPolygon(Svg, vertices.ToArray(), rayColors[i]);
            }
        }

        /// <summary>
        /// Returns a list of the raycolors, whereas each list element represents one ray
        /// </summary>
        private List<Color> GetRayColors(int numRays, RayOrigin rayOrigin)
        {
            ChosenColorScheme = GetWeightedRandomEnum<ColorScheme>(ColorSchemes);

            // Rule out invalid combinations
            while (numRays % 2 == 0 && ChosenColorScheme == ColorScheme.Symmetrical ||
                numRays > 8 && ChosenColorScheme == ColorScheme.Unique ||
                ChosenColorScheme == ColorScheme.Alternating && numRays < 4 ||
                ChosenColorScheme == ColorScheme.Alternating3 && numRays < 6 ||
                ChosenColorScheme == ColorScheme.Alternating4 && numRays < 8 ||
                rayOrigin == RayOrigin.Center && ChosenColorScheme == ColorScheme.Symmetrical ||
                rayOrigin == RayOrigin.Center && ChosenColorScheme == ColorScheme.Alternating && numRays % 2 != 0 ||
                rayOrigin == RayOrigin.Center && ChosenColorScheme == ColorScheme.Alternating3 && numRays % 3 != 0 ||
                rayOrigin == RayOrigin.Center && ChosenColorScheme == ColorScheme.Alternating4 && numRays % 4 != 0) 
            {
                ChosenColorScheme = GetWeightedRandomEnum<ColorScheme>(ColorSchemes);
            }

            if (ChosenColorScheme == ColorScheme.Unique) return ColorManager.GetRandomColors(numRays);
            else if(ChosenColorScheme == ColorScheme.Symmetrical)
            {
                int numColors = numRays / 2 + 1;
                List<Color> rayColors = new List<Color>();
                List<Color> symColors = ColorManager.GetRandomColors(numColors);
                for (int i = 0; i < numRays; i++) rayColors.Add(symColors[Math.Abs(numColors - i - 1)]);
                return rayColors;
            }
            else // alternating
            {
                List<Color> rayColors = new List<Color>();

                int numAlternateColors = 0;
                if (ChosenColorScheme == ColorScheme.Alternating) numAlternateColors = 2;
                if (ChosenColorScheme == ColorScheme.Alternating3) numAlternateColors = 3;
                if (ChosenColorScheme == ColorScheme.Alternating4) numAlternateColors = 4;
                List<Color> alternateColors = ColorManager.GetRandomColors(numAlternateColors);
                for(int i = 0; i < numRays; i++) rayColors.Add(alternateColors[i % numAlternateColors]);
                return rayColors;
            }
        }

        /// <summary>
        /// Returns the origin point of the given RayOrigin
        /// </summary>
        private Vector2 GetOriginPoint(RayOrigin rayOrigin)
        {
            if(rayOrigin == RayOrigin.TopLeftCorner) return new Vector2(0f, 0f);
            if(rayOrigin == RayOrigin.TopCenter) return new Vector2(FlagCenter.X, 0f);
            if(rayOrigin == RayOrigin.TopRightCorner) return new Vector2(FlagWidth, 0f);
            if(rayOrigin == RayOrigin.RightCenter) return new Vector2(FlagWidth, FlagCenter.Y);
            if(rayOrigin == RayOrigin.BotRightCorner) return new Vector2(FlagWidth, FlagHeight);
            if(rayOrigin == RayOrigin.BotCenter) return new Vector2(FlagCenter.X, FlagHeight);
            if(rayOrigin == RayOrigin.BotLeftCorner) return new Vector2(0f, FlagHeight);
            if(rayOrigin == RayOrigin.LeftCenter) return new Vector2(0f, FlagCenter.Y);
            if (rayOrigin == RayOrigin.Center) return FlagCenter;
            throw new Exception("RayOrigin not handled.");
        }

        private float GetAngle(int numRays, int fullAngle, int rayId, RaySpacing raySpacing)
        {
            float angleStep = 1f * fullAngle / numRays;
            float alternateAngleOffset = angleStep * AlternateAngleOffset;

            if (raySpacing == RaySpacing.Even) return rayId * angleStep;
            if( raySpacing == RaySpacing.Alternating)
            {
                if(rayId % 2 == 0) return (rayId * angleStep) + alternateAngleOffset;
                else return (rayId * angleStep) - alternateAngleOffset;
            }
            throw new Exception("RaySpacing not handled");
        }
    }
}
