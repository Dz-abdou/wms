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
  columns: (
    remove: (fieldName: number) => void,
  ) => ColumnsType<T & EditableFormListTableRow>;
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

        const tableColumns: ColumnsType<T & EditableFormListTableRow> = columns(
          remove,
        ).map((column) =>
          column.key === "actions"
            ? { ...column, fixed: "right", width: column.width ?? 120 }
            : column,
        );

        return (
          <Table
            columns={tableColumns}
            dataSource={rows}
            pagination={false}
            rowKey="key"
            scroll={scroll}
            summary={() => (
              <Table.Summary>
                <Table.Summary.Row className="editable-table-add-row">
                  <Table.Summary.Cell colSpan={tableColumns.length} index={0}>
                    <Button
                      aria-label={addLabel}
                      disabled={addDisabled}
                      onClick={addRow}
                      type="text"
                    >
                      + {addLabel}
                    </Button>
                  </Table.Summary.Cell>
                </Table.Summary.Row>
              </Table.Summary>
            )}
          />
        );
      }}
    </Form.List>
  );
}
