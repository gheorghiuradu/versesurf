using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Extensions
{
    public static class GridLayoutGroupExtensions
    {
        public static Vector2 GetColumnsAndRows(this GridLayoutGroup glg)
        {
            //Make sure all values are calculated;
            Canvas.ForceUpdateCanvases();

            var result = new Vector2(0, 0);

            if (glg.transform.childCount == 0)
                return result;

            //Column and row are now 1
            result.x = 1;
            result.y = 1;

            //Get the first child GameObject of the GridLayoutGroup
            RectTransform firstChildObj = glg.transform.
                GetChild(0).GetComponent<RectTransform>();

            Vector2 firstChildPos = firstChildObj.localPosition;
            bool stopCountingRow = false;

            //Loop through the rest of the child object
            for (int i = 1; i < glg.transform.childCount; i++)
            {
                //Get the next child
                RectTransform currentChildObj = glg.transform.
               GetChild(i).GetComponent<RectTransform>();

                Vector2 currentChildPos = currentChildObj.localPosition;

                //if first child.x == otherchild.x, it is a column, ele it's a row
                if (firstChildPos.x == currentChildPos.x)
                {
                    result.y++;
                    //Stop couting row once we find column
                    stopCountingRow = true;
                }
                else if (!stopCountingRow)
                {
                    result.x++;
                }
            }

            return result;
        }

        public static void ResizeHeightToFitChildren(this GridLayoutGroup glg)
        {
            var rows = GetColumnsAndRows(glg).y;
            var height = (rows * (glg.cellSize.y + glg.spacing.y)) + glg.padding.top + glg.padding.bottom;
            var rectTransform = glg.GetComponent<RectTransform>();
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }
    }
}