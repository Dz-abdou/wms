import { render, screen } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { describe, expect, it } from "vitest";
import { useReturnDestination } from "./returnNavigation";

function ReturnDestinationProbe({ fallback }: { fallback: string }) {
  const { returnTo } = useReturnDestination(fallback);
  return <output data-testid="return-destination">{returnTo}</output>;
}

describe("useReturnDestination", () => {
  it("uses the originating list URL when one was supplied", () => {
    render(
      <MemoryRouter
        initialEntries={[{ pathname: "/products/1", state: { returnTo: "/products?page=2" } }]}
      >
        <ReturnDestinationProbe fallback="/products" />
      </MemoryRouter>,
    );

    expect(screen.getByTestId("return-destination")).toHaveTextContent("/products?page=2");
  });

  it("uses the safe feature fallback for a direct URL", () => {
    render(
      <MemoryRouter initialEntries={["/products/1"]}>
        <ReturnDestinationProbe fallback="/products" />
      </MemoryRouter>,
    );

    expect(screen.getByTestId("return-destination")).toHaveTextContent("/products");
  });
});
