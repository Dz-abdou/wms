type QuantityDeltaProps = {
  value: number;
};

/** Displays a signed quantity using its business direction rather than a feature-specific movement type. */
export function QuantityDelta({ value }: QuantityDeltaProps) {
  const direction = value > 0 ? "positive" : value < 0 ? "negative" : "neutral";
  const displayValue = value > 0 ? `+${value}` : value;

  return (
    <span className={`quantity-delta quantity-delta-${direction}`}>
      {displayValue}
    </span>
  );
}
