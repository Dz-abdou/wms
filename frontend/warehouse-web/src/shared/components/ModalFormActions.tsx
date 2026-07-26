import { Button, Space } from "antd";

type ModalFormActionsProps = {
  cancelLabel: string;
  isSubmitting: boolean;
  onCancel: () => void;
  submitLabel: string;
};

/** Standard footer actions for short configuration dialogs. */
export function ModalFormActions({
  cancelLabel,
  isSubmitting,
  onCancel,
  submitLabel,
}: ModalFormActionsProps) {
  return (
    <div className="modal-form-actions">
      <Space>
        <Button disabled={isSubmitting} htmlType="button" onClick={onCancel}>
          {cancelLabel}
        </Button>
        <Button htmlType="submit" loading={isSubmitting} type="primary">
          {submitLabel}
        </Button>
      </Space>
    </div>
  );
}
