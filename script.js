document.addEventListener('DOMContentLoaded', () => {
    // Smooth scrolling for anchor links
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function (e) {
            e.preventDefault();
            const target = document.querySelector(this.getAttribute('href'));
            if (target) {
                target.scrollIntoView({
                    behavior: 'smooth'
                });
            }
        });
    });

    // Intersection Observer for fade-in animations
    const observerOptions = {
        root: null,
        rootMargin: '0px',
        threshold: 0.1
    };

    const observer = new IntersectionObserver((entries, observer) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('visible');
            } else {
                entry.target.classList.remove('visible');
            }
        });
    }, observerOptions);

    const fadeElements = document.querySelectorAll('.fade-in');
    fadeElements.forEach(el => observer.observe(el));
});

/* Product Selection Logic */
function selectOption(groupId, element) {
    // Find the container group
    const group = document.getElementById(groupId);

    // Remove 'selected' class from all options in this group
    const options = group.getElementsByClassName('option-card');
    for (let opt of options) {
        opt.classList.remove('selected');
    }

    // Add 'selected' class to the clicked element
    element.classList.add('selected');

    // Update product image if data-img attribute exists
    const newImgSrc = element.getAttribute('data-img');
    const productImg = document.getElementById('product-img');

    if (newImgSrc && productImg) {
        // Add fade out effect
        productImg.style.opacity = '0';

        setTimeout(() => {
            productImg.src = newImgSrc;
            productImg.onload = () => {
                productImg.style.opacity = '1';
            };
        }, 200); // Wait for fade out
    }

    console.log('Selected option in ' + groupId);
}

/* Plan Type Toggle Logic */
function togglePlan(type) {
    // Buttons
    document.getElementById('btn-sub').classList.remove('selected');
    document.getElementById('btn-pur').classList.remove('selected');

    // Views
    document.getElementById('view-sub').classList.remove('active');
    document.getElementById('view-pur').classList.remove('active');

    let activeGroupId = '';

    if (type === 'subscription') {
        document.getElementById('btn-sub').classList.add('selected');
        document.getElementById('view-sub').classList.add('active');
        activeGroupId = 'opt-sub-model';
    } else {
        document.getElementById('btn-pur').classList.add('selected');
        document.getElementById('view-pur').classList.add('active');
        activeGroupId = 'opt-pur-model';
    }

    // Sync image with the currently selected option in the active view
    const group = document.getElementById(activeGroupId);
    const selectedOption = group.querySelector('.option-card.selected');
    if (selectedOption) {
        // Trigger image update manually
        const newImgSrc = selectedOption.getAttribute('data-img');
        const productImg = document.getElementById('product-img');
        if (newImgSrc && productImg) {
            productImg.style.opacity = '0';
            setTimeout(() => {
                productImg.src = newImgSrc;
                productImg.onload = () => {
                    productImg.style.opacity = '1';
                };
            }, 200);
        }
    }
}
