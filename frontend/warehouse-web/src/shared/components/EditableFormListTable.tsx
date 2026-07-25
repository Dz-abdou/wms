import { Button, Form, Table } from "antd";
import type { ColumnsType, TableProps } from "antd/es/table";

export type EditableFormListTableRow = {
  key: number;
  fieldName: number;
};

type FormListField = Pick<EditableFormListTableRow, "key"> & {
  name: number;
};

export type EditableFormListTableActions = {
  add: () => void;
  remove: (fieldName: number) => void;
};

type EditableFormListTableProps<T extends object> = {
  columns: (actions: EditableFormListTableActions) => ColumnsType<
    T & EditableFormListTableRow
  >;
  createRow: (field: FormListField) => T;
  addLabel: string;
  addInitialValue?: unknown;
  addDisabled?: boolean;
  name: string;
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
        const addRow = () => add(addInitialValue);
        const rows = fields.map((field) => ({
          key: field.key,
          fieldName: field.name,
          ...createRow(field),
        }));

        return (
          <Table
            columns={columns({ add: addRow, remove })}
            dataSource={rows}
            locale={{
              emptyText: (
                <Button
                  aria-label={addLabel}
                  disabled={addDisabled}
                  onClick={addRow}
                  type="dashed"
                >
                  + {addLabel}
                </Button>
              ),
            }}
            pagination={false}
            rowKey="key"
            scroll={scroll}
          />
        );
      }}
    </Form.List>
  );
}
