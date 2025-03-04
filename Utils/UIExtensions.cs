using UnityEngine;
using UnityEngine.UI;

namespace Telegraphist.Utils
{
    public enum RectType
    {
        Min = 0,
        Center = 1,
        Max = 2
    }

    public static class UIExtensions
    {
        // Shared array used to receive result of RectTransform.GetWorldCorners
        static Vector3[] corners = new Vector3[4];

        /// <summary>
        /// Transform the bounds of the current rect transform to the space of another transform.
        /// </summary>
        /// <param name="source">The rect to transform</param>
        /// <param name="target">The target space to transform to</param>
        /// <returns>The transformed bounds</returns>
        public static Bounds TransformBoundsTo(this RectTransform source, Transform target)
        {
            // Based on code in ScrollRect's internal GetBounds and InternalGetBounds methods
            var bounds = new Bounds();
            if (source != null)
            {
                source.GetWorldCorners(corners);

                var vMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
                var vMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);

                var matrix = target.worldToLocalMatrix;
                for (int j = 0; j < 4; j++)
                {
                    Vector3 v = matrix.MultiplyPoint3x4(corners[j]);
                    vMin = Vector3.Min(v, vMin);
                    vMax = Vector3.Max(v, vMax);
                }

                bounds = new Bounds(vMin, Vector3.zero);
                bounds.Encapsulate(vMax);
            }
            return bounds;
        }

        /// <summary>
        /// Normalize a distance to be used in verticalNormalizedPosition or horizontalNormalizedPosition.
        /// </summary>
        /// <param name="axis">Scroll axis, 0 = horizontal, 1 = vertical</param>
        /// <param name="distance">The distance in the scroll rect's view's coordiante space</param>
        /// <returns>The normalized scoll distance</returns>
        public static float NormalizeScrollDistance(this ScrollRect scrollRect, int axis, float distance)
        {
            // Based on code in ScrollRect's internal SetNormalizedPosition method
            var viewport = scrollRect.viewport;
            var viewRect = viewport != null ? viewport : scrollRect.GetComponent<RectTransform>();
            var viewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);

            var content = scrollRect.content;
            var contentBounds = content != null ? content.TransformBoundsTo(viewRect) : new Bounds();

            var hiddenLength = contentBounds.size[axis] - viewBounds.size[axis];
            return distance / hiddenLength;
        }

        public static void ScrollToTarget(this ScrollRect scrollRect, RectTransform target, RectType rectType, RectTransform.Axis axis = RectTransform.Axis.Vertical)
        {
            // The scroll rect's view's space is used to calculate scroll position
            var view = scrollRect.viewport ?? scrollRect.GetComponent<RectTransform>();

            // Calcualte the scroll offset in the view's space
            var viewRect = view.rect;
            var elementBounds = target.TransformBoundsTo(view);

            // Normalize and apply the calculated offset
            if (axis == RectTransform.Axis.Vertical)
            {
                var offset = viewRect.GetRectFromType(rectType).y - elementBounds.GetBoundsFromType(rectType).y;
                var scrollPos = scrollRect.verticalNormalizedPosition - scrollRect.NormalizeScrollDistance(1, offset);
                scrollRect.verticalNormalizedPosition = Mathf.Clamp01(scrollPos);
            }
            else
            {
                var offset = viewRect.GetRectFromType(rectType).x - elementBounds.GetBoundsFromType(rectType).x;
                var scrollPos = scrollRect.horizontalNormalizedPosition - scrollRect.NormalizeScrollDistance(0, offset);
                scrollRect.horizontalNormalizedPosition = Mathf.Clamp01(scrollPos);
            }
        }

        private static Vector2 GetBoundsFromType(this Bounds bounds, RectType rectType)
        {
            return rectType switch
            {
                RectType.Min => bounds.min,
                RectType.Max => bounds.max,
                RectType.Center => bounds.center,
                _ => bounds.min,
            };
        }

        private static Vector2 GetRectFromType(this Rect rect, RectType rectType)
        {
            return rectType switch
            {
                RectType.Min => rect.min,
                RectType.Max => rect.max,
                RectType.Center => rect.center,
                _ => rect.min,
            };
        }

    }
}
