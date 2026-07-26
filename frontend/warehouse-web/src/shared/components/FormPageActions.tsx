import { Button } from "antd";
import { useTranslation } from "react-i18next";

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
  const { t } = useTranslation();
  const confirmCancel = () => {
    if (
      window.confirm(
        `${t("ui.discardChangesTitle")}\n\n${t("ui.discardChangesDescription")}`,
      )
    ) {
      onCancel?.();
    }
  };

  return (
    <div className="form-page-actions">
      {cancelLabel && onCancel ? (
        <Button
          disabled={isSubmitting}
          htmlType="button"
          onClick={confirmCancel}
        >
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
