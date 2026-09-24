window.submitLogoutForm = (returnUrl) => {
    const form = document.getElementById('logoutForm');
    form.elements.namedItem('ReturnUrl').value = returnUrl;
    form.submit();
};
