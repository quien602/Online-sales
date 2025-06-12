// Hiệu ứng khi di chuột vào sản phẩm
function zoomInProduct(product) {
    product.querySelector('img').style.transform = 'scale(1.1)';
}

function zoomOutProduct(product) {
    product.querySelector('img').style.transform = 'scale(1)';
}

// Hiệu ứng khi di chuột vào loại sản phẩm
function animateCategory(category) {
    category.querySelector('img').style.transform = 'scale(1.1)';
}

function resetCategory(category) {
    category.querySelector('img').style.transform = 'scale(1)';
}

// Lắng nghe sự kiện khi di chuột vào sản phẩm
const products = document.querySelectorAll('.product');
products.forEach(product => {
    product.addEventListener('mouseenter', () => {
        zoomInProduct(product);
    });

    product.addEventListener('mouseleave', () => {
        zoomOutProduct(product);
    });
});

// Lắng nghe sự kiện khi di chuột vào loại sản phẩm
const categories = document.querySelectorAll('.category');
categories.forEach(category => {
    category.addEventListener('mouseenter', () => {
        animateCategory(category);
    });

    category.addEventListener('mouseleave', () => {
        resetCategory(category);
    });
});
