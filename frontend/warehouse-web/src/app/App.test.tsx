import { render, screen } from "@testing-library/react";
import { App } from "./App";

describe("App", () => {
  it("redirects an unauthenticated visitor to the localized login page", async () => {
    render(<App />);

    expect(
      await screen.findByRole("heading", { name: "Sign in" }),
    ).toBeInTheDocument();
  });
});
