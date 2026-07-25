import { Button, Form, Table } from "antd";
import type { ColumnsType, TableProps } from "antd/es/table";

export type EditableFormListTableRow = {
  key: number;
  fieldName: number;
};

type FormListField = Pick<EditableFormListTableRow, "key"> & {
  name: number;
};

type EditableFormListTableProps<T extends object> = {
  name: string;
  columns: (
    remove: (fieldName: number) => void,
  ) => ColumnsType<T & EditableFormListTableRow>;
  createRow: (field: FormListField) => T;
  addLabel: string;
  addInitialValue?: unknown;
  addDisabled?: boolean;
  scroll?: TableProps<T & EditableFormListTableRow>["scroll"];
};

/**
 * Presents a dynamic Ant Design Form.List as an editable business table.
 * Features retain ownership of columns, field names, validation, and row data.
 */
export function EditableFormListTable<T extends object>({
  name,
  columns,
  createRow,
  addLabel,
  addInitialValue,
  addDisabled = false,
  scroll,
}: EditableFormListTableProps<T>) {
  return (
    <Form.List name={name}>
      {(fields, { add, remove }) => {
        const rows = fields.map((field) => ({
          key: field.key,
          fieldName: field.name,
          ...createRow(field),
        }));

        return (
          <>
            <Table
              columns={columns(remove)}
              dataSource={rows}
              pagination={false}
              rowKey="key"
              scroll={scroll}
            />
            <Button
              disabled={addDisabled}
              onClick={() => add(addInitialValue)}
              type="dashed"
            >
              {addLabel}
            </Button>
          </>
        );
      }}
    </Form.List>
  );
}
