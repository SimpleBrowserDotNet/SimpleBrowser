namespace SimpleBrowser
{
	public enum FindBy
	{
		Name,
		Id,
		Class,
		Value,
		Text,
		PartialText,
		PartialName,
		PartialClass,
		PartialValue,
		PartialId
	}

	public enum ElementType
	{
		Anchor,
		TextField,
		Button,
		RadioButton,
		Checkbox,
		SelectBox,
		Script
	}

	public enum ClickResult
	{
		Failed,
		SucceededNoOp,
		SucceededNoNavigation,
		SucceededNavigationComplete,
		SucceededNavigationError
	}
}