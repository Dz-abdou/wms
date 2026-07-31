import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { QuantityDelta } from "../../components/QuantityDelta";

describe("QuantityDelta", () => {
  it("uses semantic direction styles for positive, negative, and zero values", () => {
    render(
      <>
        <QuantityDelta value={4} />
        <QuantityDelta value={-2} />
        <QuantityDelta value={0} />
      </>,
    );

    expect(screen.getByText("+4")).toHaveClass("quantity-delta-positive");
    expect(screen.getByText("-2")).toHaveClass("quantity-delta-negative");
    expect(screen.getByText("0")).toHaveClass("quantity-delta-neutral");
  });
});
