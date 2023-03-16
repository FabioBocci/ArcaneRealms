using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class ArchLayoutGroup : HorizontalOrVerticalLayoutGroup {

	[Header("custom Stuff")]
	[SerializeField] private float archWidth = 700;
	[SerializeField] private float archHeight = 400;
	[SerializeField] private float curveRadius = 200;
	[SerializeField] private float angleOut = Mathf.PI;

	protected override void OnValidate() {
		base.OnValidate();
	}

	public override void CalculateLayoutInputVertical() {
		base.CalculateLayoutInputHorizontal();
	}

	public override void SetLayoutHorizontal() {
		SetLayout();
	}

	public override void SetLayoutVertical() {
		SetLayout();
	}

	public void SetLayout() {
		int childCount = rectChildren.Count;
		if(childCount == 0)
			return;

		float childWidth = rectChildren[0].rect.width;
		float childHeight = rectChildren[0].rect.height;

		float totalWidth = childWidth * childCount;
		float totalHeight = childHeight;

		float angleDelta = angleOut / (childCount + 1);

		for(int i = childCount - 1; i >= 0; i--) {
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

			child.anchoredPosition = new Vector2(x + archWidth / 2, y);

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

}
