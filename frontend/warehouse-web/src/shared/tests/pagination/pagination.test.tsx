import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter, useLocation } from "react-router-dom";
import { describe, expect, it } from "vitest";
import { useUrlListQuery } from "../../pagination/pagination";

function QueryProbe() {
  const query = useUrlListQuery();
  const location = useLocation();
  return (
    <>
      <output data-testid="search">{location.search}</output>
      <button onClick={() => query.update({ q: "SKU-001", active: true })}>
        Filter
      </button>
      <button onClick={() => query.update({ page: 2 }, false)}>
        Next page
      </button>
    </>
  );
}

describe("useUrlListQuery", () => {
  it("serializes filters and resets pagination while preserving page size", async () => {
    const user = userEvent.setup();
    render(
      <MemoryRouter initialEntries={["/products?page=3&pageSize=50"]}>
        <QueryProbe />
      </MemoryRouter>,
    );

    await user.click(screen.getByRole("button", { name: "Filter" }));

    expect(screen.getByTestId("search").textContent).toBe(
      "?pageSize=50&q=SKU-001&active=true",
    );
  });

  it("updates only pagination without removing active filters", async () => {
    const user = userEvent.setup();
    render(
      <MemoryRouter initialEntries={["/products?q=SKU-001&active=true"]}>
        <QueryProbe />
      </MemoryRouter>,
    );

    await user.click(screen.getByRole("button", { name: "Next page" }));

    expect(screen.getByTestId("search").textContent).toBe(
      "?q=SKU-001&active=true&page=2",
    );
  });
});
