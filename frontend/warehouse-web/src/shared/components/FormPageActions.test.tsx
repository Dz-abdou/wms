import { Form } from "antd";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import type { FormEvent } from "react";
import { describe, expect, it, vi } from "vitest";
import { FormPageActions } from "./FormPageActions";

describe("FormPageActions", () => {
  it("places cancel and submit actions in the shared form footer", async () => {
    const user = userEvent.setup();
    const onCancel = vi.fn();
    const onSubmit = vi.fn((event: FormEvent) => event.preventDefault());
    vi.spyOn(window, "confirm").mockReturnValue(true);

    render(
      <Form onSubmitCapture={onSubmit}>
        <FormPageActions
          cancelLabel="Cancel"
          isSubmitting={false}
          onCancel={onCancel}
          submitLabel="Save changes"
        />
      </Form>,
    );

    await user.click(screen.getByRole("button", { name: "Cancel" }));
    expect(onCancel).toHaveBeenCalledOnce();

    await user.click(screen.getByRole("button", { name: "Save changes" }));
    expect(onSubmit).toHaveBeenCalledOnce();
  });
});
