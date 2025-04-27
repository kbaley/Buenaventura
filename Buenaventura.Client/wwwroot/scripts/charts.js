function formatMoneyAxis(value) {
    if (Math.abs(value) < 1000) {
        return value;
    }
    
    if (Math.abs(value) < 10000) {
        return (value / 1000).toFixed(1) + ' K';
    }
    
    return (value / 1000).toFixed(0) + ' K';
}