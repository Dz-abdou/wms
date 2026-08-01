import { Button, Input, Space, Tooltip } from "antd";
import { ReloadOutlined } from "@ant-design/icons";

type ReloadableQuantityFieldProps = {
  disabled?: boolean;
  label: string;
  onChange?: (value: number | undefined) => void;
  onReload: () => void;
  reloadLabel: string;
  unitOfMeasure?: string;
  value?: number;
};

export function ReloadableQuantityField({
  disabled,
  label,
  onReload,
  reloadLabel,
  unitOfMeasure,
  value,
}: ReloadableQuantityFieldProps) {
  const displayValue =
    value === undefined
      ? undefined
      : unitOfMeasure
        ? `${value} ${unitOfMeasure}`
        : String(value);

  return (
    <Space.Compact block>
      <Input aria-label={label} disabled placeholder="—" value={displayValue} />
      <Tooltip title={reloadLabel}>
        <Button
          aria-label={reloadLabel}
          disabled={disabled}
          icon={<ReloadOutlined />}
          onClick={onReload}
        />
      </Tooltip>
    </Space.Compact>
  );
}
