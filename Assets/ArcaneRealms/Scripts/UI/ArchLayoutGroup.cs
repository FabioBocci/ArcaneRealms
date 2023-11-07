using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ArcaneRealms.Scripts.UI
{
	[ExecuteInEditMode]
	public class ArchLayoutGroup : HorizontalOrVerticalLayoutGroup {

		[Header("custom Stuff")]
		[SerializeField] private float archWidth = 700;
		[SerializeField] private float archHeight = 400;
		[SerializeField] private float curveRadius = 200;
		[SerializeField] private float angleOut = Mathf.PI;

		public override void CalculateLayoutInputVertical() {
			base.CalculateLayoutInputHorizontal();
		}

		public override void SetLayoutHorizontal() {
			SetLayout();
		}

		public override void SetLayoutVertical() {
			SetLayout();
		}

		public void SetLayout()
		{
		    int childCount = rectChildren.Count;
		    if (childCount == 0)
		        return;

		    float childWidth = rectChildren[0].rect.width;
		    float childHeight = rectChildren[0].rect.height;

		    float totalWidth = childWidth * childCount;
		    float totalHeight = childHeight;

		    float angleDelta = angleOut / (childCount + 1);

		    for (int i = childCount - 1; i >= 0; i--)
		    {
			    float angle = angleDelta * (childCount - i) - angleOut / 2;
			    float x = Mathf.Sin(angle) * curveRadius;
			    // Calculate the y position with an offset based on the card's index
			    float y = (1 - Mathf.Cos(angle)) * archHeight / 2;

			    RectTransform child = rectChildren[i];

			    Vector2 originalAnchorMin = child.anchorMin;
			    Vector2 originalAnchorMax = child.anchorMax;
			    Vector2 originalPivot = child.pivot;

			    child.anchorMin = new Vector2(0, 0.5f);
			    child.anchorMax = new Vector2(0, 0.5f);
			    child.pivot = new Vector2(0, 0.5f);

			    float targetX = x + archWidth / 2;
			    Vector2 targetPosition = new Vector2(targetX, y);

			    if (!Application.isPlaying)
			    {
				    // Set the position instantly if in edit mode
				    child.anchoredPosition = targetPosition;
			    }
			    else
			    {
				    // Animate the position in play mode
				    StartCoroutine(AnimatePosition(child, targetPosition));
			    }

			    child.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, childWidth);
			    child.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, childHeight);

			    child.localRotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg);

			    // Set the original anchor and pivot values
			    child.anchorMin = originalAnchorMin;
			    child.anchorMax = originalAnchorMax;
			    child.pivot = originalPivot;
		    }

		    float paddingLeft = padding.left;
		    float paddingRight = padding.right;
		    float paddingTop = padding.top;
		    float paddingBottom = padding.bottom;

		    float totalPaddingWidth = paddingLeft + paddingRight;
		    float totalPaddingHeight = paddingTop + paddingBottom;

		    SetLayoutInputForAxis(totalWidth + totalPaddingWidth, totalWidth + totalPaddingWidth, -1, 0);
		    SetLayoutInputForAxis(totalHeight + totalPaddingHeight, totalHeight + totalPaddingHeight, -1, 1);
		}

		private IEnumerator AnimatePosition(RectTransform rectTransform, Vector2 targetPosition, float duration = 0.5f)
		{
		    Vector2 startPosition = rectTransform.anchoredPosition;
		    float startTime = Time.time;

		    while (Time.time - startTime < duration)
		    {
		        float t = (Time.time - startTime) / duration;
		        rectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, t);
		        yield return null;
		    }

		    rectTransform.anchoredPosition = targetPosition;
		}
	}
}
