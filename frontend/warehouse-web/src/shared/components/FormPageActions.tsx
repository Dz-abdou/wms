import { Button } from "antd";

type FormPageActionsProps = {
  cancelLabel?: string;
  isSubmitting: boolean;
  onCancel?: () => void;
  submitLabel: string;
};

/** Standard end-of-form actions for create and edit pages. */
export function FormPageActions({
  cancelLabel,
  isSubmitting,
  onCancel,
  submitLabel,
}: FormPageActionsProps) {
  return (
    <div className="form-page-actions">
      {cancelLabel && onCancel ? (
        <Button disabled={isSubmitting} htmlType="button" onClick={onCancel}>
          {cancelLabel}
        </Button>
      ) : (
        <span />
      )}
      <Button htmlType="submit" loading={isSubmitting} type="primary">
        {submitLabel}
      </Button>
    </div>
  );
}
