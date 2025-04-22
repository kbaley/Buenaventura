function formatMoneyAxis(value) {
    if (Math.abs(value) < 1000) {
        return value;
    }
    
    return (value / 1000).toFixed(0) + ' K';
}