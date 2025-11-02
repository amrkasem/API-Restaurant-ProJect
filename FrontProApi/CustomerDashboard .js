 
        // API Configuration
        const API_ENDPOINTS = [
            'https://localhost:7201/api',
            'http://localhost:5064/api',
            'http://localhost:32080/api'
        ];
        
        let API_BASE = '';
        let authToken = localStorage.getItem('jwt_token');
        let isAPIConnected = false;
        let currentUser = null;

        // Initialize
        document.addEventListener('DOMContentLoaded', async function() {
            // Check authentication
            if (!authToken) {
                console.error(' No authentication token found');
                alert('Please login first!');
                window.location.href = 'login.html';
                return;
            }
            
            // Update time
            updateTime();
            setInterval(updateTime, 1000);
            
            // Find working API
            await findWorkingAPIEndpoint();
            
            if (!isAPIConnected) {
                showAPIError();
                return;
            }
            
            // Load data
            await loadDashboardData();
        });

        function updateTime() {
            const now = new Date();
            document.getElementById('currentTime').textContent = now.toLocaleTimeString('en-US', {
                hour: '2-digit',
                minute: '2-digit',
                second: '2-digit'
            });
        }

        // API Connection
        async function findWorkingAPIEndpoint() {
            for (const endpoint of API_ENDPOINTS) {
                try {
                    console.log(`🔍 Testing: ${endpoint}`);
                    
                    const response = await fetch(`${endpoint}/customer/dashboard`, {
                        method: 'GET',
                        headers: {
                            'Authorization': `Bearer ${authToken}`,
                            'Content-Type': 'application/json'
                        }
                    });
                    
                    if (response.ok) {
                        API_BASE = endpoint;
                        isAPIConnected = true;
                        showAPIConnected();
                        console.log(` API connected: ${endpoint}`);
                        return;
                    }
                } catch (error) {
                    console.error(` Failed: ${endpoint}`, error.message);
                }
            }
            
            showAPIError();
        }

        function showAPIError() {
            document.getElementById('apiStatus').innerHTML = `
                <strong> API Connection Failed</strong> - Cannot connect to backend
                <br><small>Make sure your API is running</small>
                <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
            `;
            document.getElementById('apiStatus').className = 'alert alert-danger alert-dismissible fade show';
        }

        function showAPIConnected() {
            document.getElementById('apiStatus').innerHTML = `
                <strong> Connected</strong> - ${API_BASE}
                <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
            `;
            document.getElementById('apiStatus').className = 'alert alert-success alert-dismissible fade show';
        }

        // Navigation
        function showSection(section) {
            document.querySelectorAll('.nav-link').forEach(link => link.classList.remove('active'));
            event.currentTarget.classList.add('active');

            showLoading();
            
            const titles = {
                'dashboard': 'Dashboard',
                'products': 'Browse Menu',
                'cart': 'My Shopping Cart',
                'wishlist': 'My Wishlist',
                'orders': 'My Orders',
                'profile': 'My Profile'
            };
            document.getElementById('sectionTitle').textContent = titles[section] || section;

            setTimeout(() => {
                hideLoading();
                if (section === 'dashboard') {
                    document.getElementById('dashboardSection').style.display = 'block';
                    document.getElementById('dynamicContent').innerHTML = '';
                } else {
                    document.getElementById('dashboardSection').style.display = 'none';
                    loadSectionContent(section);
                }
            }, 500);
        }

        function showLoading() {
            document.getElementById('loadingSpinner').style.display = 'block';
        }

        function hideLoading() {
            document.getElementById('loadingSpinner').style.display = 'none';
        }

        // Dashboard Data
        async function loadDashboardData() {
            try {
                // Load dashboard stats
                const dashResponse = await fetch(`${API_BASE}/customer/dashboard`, {
                    headers: {
                        'Authorization': `Bearer ${authToken}`,
                        'Content-Type': 'application/json'
                    }
                });

                if (dashResponse.ok) {
                    const result = await dashResponse.json();
                    if (result.success) {
                        const data = result.data;
                        document.getElementById('userName').textContent = data.userName;
                        document.getElementById('userEmail').textContent = data.email;
                        document.getElementById('ordersCount').textContent = data.totalOrders;
                    }
                }

                // Load cart count
                await updateCartCount();
                
                // Load wishlist count
                await updateWishlistCount();

                // Load recent orders
                await loadRecentOrders();

            } catch (error) {
                console.error(' Error loading dashboard:', error);
            }
        }

        async function updateCartCount() {
            try {
                const response = await fetch(`${API_BASE}/customer/cart/count`, {
                    headers: {
                        'Authorization': `Bearer ${authToken}`,
                        'Content-Type': 'application/json'
                    }
                });

                if (response.ok) {
                    const result = await response.json();
                    const count = result.data || 0;
                    document.getElementById('cartCount').textContent = count;
                    document.getElementById('cartBadge').textContent = count;
                }
            } catch (error) {
                console.error('Error loading cart count:', error);
            }
        }

        async function updateWishlistCount() {
            try {
                const response = await fetch(`${API_BASE}/customer/wishlists/count`, {
                    headers: {
                        'Authorization': `Bearer ${authToken}`,
                        'Content-Type': 'application/json'
                    }
                });

                if (response.ok) {
                    const result = await response.json();
                    const count = result.data || 0;
                    document.getElementById('wishlistCount').textContent = count;
                    document.getElementById('wishlistBadge').textContent = count;
                }
            } catch (error) {
                console.error('Error loading wishlist count:', error);
            }
        }

        async function loadRecentOrders() {
            try {
                const response = await fetch(`${API_BASE}/customer/orders`, {
                    headers: {
                        'Authorization': `Bearer ${authToken}`,
                        'Content-Type': 'application/json'
                    }
                });

                if (response.ok) {
                    const result = await response.json();
                    if (result.success && result.data) {
                        const orders = result.data.slice(0, 3);
                        displayRecentOrders(orders);
                    }
                }
            } catch (error) {
                console.error('Error loading recent orders:', error);
            }
        }

        function displayRecentOrders(orders) {
            const container = document.getElementById('recentOrdersContainer');
            
            if (!orders || orders.length === 0) {
                container.innerHTML = '<div class="empty-state"><i class="fas fa-receipt"></i><h4>No orders yet</h4><p>Start ordering to see your history here</p></div>';
                return;
            }

            container.innerHTML = orders.map(order => `
                <div class="order-card">
                    <div class="order-header">
                        <div>
                            <div class="order-id">Order #${order.id}</div>
                            <small class="text-muted">${new Date(order.createdAt).toLocaleDateString()}</small>
                        </div>
                        <div>
                            <span class="badge ${getStatusBadge(order.status)}">${order.status}</span>
                            <div class="mt-2"><strong>$${order.total.toFixed(2)}</strong></div>
                        </div>
                    </div>
                </div>
            `).join('');
        }

        function getStatusBadge(status) {
            const badges = {
                'Pending': 'bg-warning',
                'Confirmed': 'bg-info',
                'Preparing': 'bg-primary',
                'Ready': 'bg-success',
                'Delivered': 'bg-success',
                'Cancelled': 'bg-danger'
            };
            return badges[status] || 'bg-secondary';
        }

        // Section Content Loading
        async function loadSectionContent(section) {
            if (!isAPIConnected) {
                document.getElementById('dynamicContent').innerHTML = `
                    <div class="alert alert-danger text-center">
                        <i class="fas fa-exclamation-triangle fa-2x mb-3"></i>
                        <h5>Cannot Load ${section}</h5>
                        <p>API server is not connected</p>
                    </div>
                `;
                return;
            }

            try {
                let endpoint = '';
                switch(section) {
                    case 'products':
                        endpoint = `${API_BASE}/customer/products`;
                        break;
                    case 'cart':
                        endpoint = `${API_BASE}/customer/cart`;
                        break;
                    case 'wishlist':
                        endpoint = `${API_BASE}/customer/wishlists`;
                        break;
                    case 'orders':
                        endpoint = `${API_BASE}/customer/orders`;
                        break;
                    case 'profile':
                        displayProfile();
                        return;
                    default:
                        throw new Error(`Unknown section: ${section}`);
                }

                console.log(`🔍 Loading ${section} from:`, endpoint);

                const response = await fetch(endpoint, {
                    headers: {
                        'Authorization': `Bearer ${authToken}`,
                        'Content-Type': 'application/json'
                    }
                });

                if (response.ok) {
                    const result = await response.json();
                    console.log(` Data for ${section}:`, result);
                    
                    if (result.success) {
                        displaySectionData(section, result.data);
                        return;
                    }
                }
                
                throw new Error(`API request failed with status ${response.status}`);
            } catch (error) {
                console.error(` Error loading ${section}:`, error);
                document.getElementById('dynamicContent').innerHTML = `
                    <div class="alert alert-danger">
                        <h5><i class="fas fa-exclamation-triangle me-2"></i>Error Loading ${section}</h5>
                        <p>${error.message}</p>
                        <button class="btn btn-primary mt-2" onclick="loadSectionContent('${section}')">
                            <i class="fas fa-sync-alt me-2"></i>Retry
                        </button>
                    </div>
                `;
            }
        }

        function displaySectionData(section, data) {
            switch(section) {
                case 'products':
                    displayProducts(data);
                    break;
                case 'cart':
                    displayCart(data);
                    break;
                case 'wishlist':
                    displayWishlist(data);
                    break;
                case 'orders':
                    displayOrders(data);
                    break;
            }
        }

        // Products Display
        function displayProducts(products) {
            if (!products || products.length === 0) {
                document.getElementById('dynamicContent').innerHTML = `
                    <div class="empty-state">
                        <i class="fas fa-utensils"></i>
                        <h4>No Products Available</h4>
                        <p>Check back later for our menu</p>
                    </div>
                `;
                return;
            }

            const html = `
                <div class="content-section">
                    <div class="search-filter">
                        <div class="row">
                            <div class="col-md-8">
                                <input type="text" class="form-control" id="searchInput" placeholder="Search products..." onkeyup="filterProducts()">
                            </div>
                            <div class="col-md-4">
                                <select class="form-control" id="categoryFilter" onchange="filterProducts()">
                                    <option value="">All Categories</option>
                                    ${getUniqueCategories(products).map(cat => `<option value="${cat}">${cat}</option>`).join('')}
                                </select>
                            </div>
                        </div>
                    </div>
                    
                    <div class="row" id="productsGrid">
                        ${products.map(product => `
                            <div class="col-lg-3 col-md-4 col-sm-6 product-item" data-name="${product.name}" data-category="${product.categoryName}">
                                <div class="product-card">
                                    <img src="${product.imageUrl || 'https://via.placeholder.com/300'}" alt="${product.name}">
                                    <div class="card-body">
                                        <span class="category">${product.categoryName}</span>
                                        <h5>${product.name}</h5>
                                        <p class="text-muted small">${product.description || 'Delicious food item'}</p>
                                        <div class="price">${product.price.toFixed(2)}</div>
                                        <div class="d-flex gap-2 mt-3">
                                            <button class="btn btn-custom flex-fill" onclick="addToCart(${product.id})">
                                                <i class="fas fa-cart-plus"></i> Add to Cart
                                            </button>
                                            <button class="btn btn-outline-danger" onclick="addToWishlist(${product.id})">
                                                <i class="fas fa-heart"></i>
                                            </button>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        `).join('')}
                    </div>
                </div>
            `;
            
            document.getElementById('dynamicContent').innerHTML = html;
        }

        function getUniqueCategories(products) {
            return [...new Set(products.map(p => p.categoryName))];
        }

        function filterProducts() {
            const search = document.getElementById('searchInput').value.toLowerCase();
            const category = document.getElementById('categoryFilter').value;
            
            document.querySelectorAll('.product-item').forEach(item => {
                const name = item.dataset.name.toLowerCase();
                const cat = item.dataset.category;
                
                const matchSearch = name.includes(search);
                const matchCategory = !category || cat === category;
                
                item.style.display = (matchSearch && matchCategory) ? 'block' : 'none';
            });
        }

        // Cart Display
function displayCart(cartData) {
    if (!cartData.items || cartData.items.length === 0) {
        document.getElementById('dynamicContent').innerHTML = `
            <div class="empty-state">
                <i class="fas fa-shopping-cart"></i>
                <h4>Your Cart is Empty</h4>
                <p>Add some items to get started</p>
                <button class="btn btn-custom mt-3" onclick="showSection('products')">
                    <i class="fas fa-utensils"></i> Browse Menu
                </button>
            </div>
        `;
        return;
    }

    const html = `
        <div class="content-section">
            <h5 class="section-title">
                <i class="fas fa-shopping-cart"></i> Shopping Cart
            </h5>
            
            <div id="cartItems">
                ${cartData.items.map(item => `
                    <div class="cart-item">
                        <img src="${item.menuItemImageUrl || 'https://via.placeholder.com/100'}" alt="${item.menuItemName}">
                        <div class="details">
                            <h5>${item.menuItemName}</h5>
                            <p class="price">${item.price.toFixed(2)} each</p>
                        </div>
                        <div class="quantity-control">
                            <button onclick="updateCartQuantity(${item.id}, ${item.quantity - 1})">-</button>
                            <input type="number" value="${item.quantity}" readonly>
                            <button onclick="updateCartQuantity(${item.id}, ${item.quantity + 1})">+</button>
                        </div>
                        <div>
                            <div class="price">${item.subtotal.toFixed(2)}</div>
                            <button class="btn btn-sm btn-danger mt-2" onclick="removeFromCart(${item.id})">
                                <i class="fas fa-trash"></i>
                            </button>
                        </div>
                    </div>
                `).join('')}
            </div>

            <!-- Checkout Form -->
            <div class="mt-5 p-4 border rounded" style="background: #f8f9fa;">
                <h5 class="mb-4"><i class="fas fa-clipboard-list"></i> Complete Your Order</h5>
                
                <form id="checkoutForm">
                    <div class="row g-3">
                        <div class="col-md-6">
                            <label class="form-label">Full Name *</label>
                            <input type="text" class="form-control" id="customerName" required 
                                   value="${localStorage.getItem('user_name') || ''}" placeholder="Enter your name">
                        </div>
                        <div class="col-md-6">
                            <label class="form-label">Phone Number *</label>
                            <input type="tel" class="form-control" id="phoneNumber" required 
                                   value="${localStorage.getItem('user_phone') || ''}" placeholder="01xxxxxxxxx">
                        </div>

                        <div class="col-md-6">
                            <label class="form-label">Order Type *</label>
                            <select class="form-select" id="orderType" required>
                                <option value="1">Delivery</option>
                                <option value="0">Dine In</option>
                                <option value="2">Takeaway</option>
                            </select>
                        </div>

                        <div class="col-md-6">
                            <label class="form-label">Payment Method *</label>
                           <select class="form-select" id="paymentMethod" required>
                                <option value="1">Cash on Delivery</option>
                                <option value="2">Credit Card</option>
                                <option value="3">Debit Card</option>
                                <option value="4">Online Payment</option>
                            </select>
                        </div>

                        <div class="col-12" id="deliveryAddressGroup">
                            <label class="form-label">Delivery Address *</label>
                            <input type="text" class="form-control" id="deliveryAddress" 
                                   placeholder="123 Main St, Apt 4B, Cairo">
                        </div>

                        <div class="col-12">
                            <label class="form-label">Notes (Optional)</label>
                            <textarea class="form-control" id="orderNotes" rows="2" 
                                      placeholder="e.g., No onions, call before delivery"></textarea>
                        </div>
                    </div>

                    <hr class="my-4">

                    <div class="d-flex justify-content-between align-items-center mb-3">
                        <h5>Total Amount:</h5>
                        <h4 class="text-warning">${cartData.total.toFixed(2)}</h4>
                    </div>

                    <button type="button" class="btn btn-custom w-100" onclick="proceedToCheckout()">
                        <i class="fas fa-check"></i> Confirm & Place Order
                    </button>
                </form>
            </div>
        </div>
    `;
    
    document.getElementById('dynamicContent').innerHTML = html;

    // Hide address if not delivery
    document.getElementById('orderType').addEventListener('change', function() {
        const addressGroup = document.getElementById('deliveryAddressGroup');
        addressGroup.style.display = this.value === '1' ? 'block' : 'none';
        document.getElementById('deliveryAddress').required = this.value === '1';
    });

    // Trigger initial state
    document.getElementById('orderType').dispatchEvent(new Event('change'));
}
        // Wishlist Display
        function displayWishlist(wishlistData) {
            if (!wishlistData.items || wishlistData.items.length === 0) {
                document.getElementById('dynamicContent').innerHTML = `
                    <div class="empty-state">
                        <i class="fas fa-heart"></i>
                        <h4>Your Wishlist is Empty</h4>
                        <p>Save items for later</p>
                        <button class="btn btn-custom mt-3" onclick="showSection('products')">
                            <i class="fas fa-utensils"></i> Browse Menu
                        </button>
                    </div>
                `;
                return;
            }

            const html = `
                <div class="content-section">
                    <h5 class="section-title">
                        <i class="fas fa-heart"></i> My Wishlist
                    </h5>
                    
                    <div>
                        ${wishlistData.items.map(item => `
                            <div class="wishlist-item">
                                <img src="${item.menuItemImageUrl || 'https://via.placeholder.com/100'}" alt="${item.menuItemName}">
                                <div class="details">
                                    <h5>${item.menuItemName}</h5>
                                    <span class="category">${item.categoryName}</span>
                                    <p class="price mt-2">${item.price.toFixed(2)}</p>
                                    <small class="text-muted">Quantity: ${item.desiredQuantity}</small>
                                </div>
                                <div class="d-flex flex-column gap-2">
                                    <button class="btn btn-custom" onclick="moveToCart(${item.menuItemId})">
                                        <i class="fas fa-cart-plus"></i> Add to Cart
                                    </button>
                                    <button class="btn btn-danger" onclick="removeFromWishlist(${item.id})">
                                        <i class="fas fa-trash"></i> Remove
                                    </button>
                                </div>
                            </div>
                        `).join('')}
                    </div>
                    
                    <div class="mt-4 p-4" style="background: #f9fafb; border-radius: 12px;">
                        <div class="d-flex justify-content-between">
                            <h5>Estimated Total:</h5>
                            <h4 class="text-warning">${wishlistData.totalEstimatedPrice.toFixed(2)}</h4>
                        </div>
                    </div>
                </div>
            `;
            
            document.getElementById('dynamicContent').innerHTML = html;
        }

        // Orders Display
        function displayOrders(orders) {
            if (!orders || orders.length === 0) {
                document.getElementById('dynamicContent').innerHTML = `
                    <div class="empty-state">
                        <i class="fas fa-receipt"></i>
                        <h4>No Orders Yet</h4>
                        <p>Start ordering to see your history</p>
                        <button class="btn btn-custom mt-3" onclick="showSection('products')">
                            <i class="fas fa-utensils"></i> Order Now
                        </button>
                    </div>
                `;
                return;
            }

            const html = `
                <div class="content-section">
                    <h5 class="section-title">
                        <i class="fas fa-receipt"></i> Order History
                    </h5>
                    
                    ${orders.map(order => `
                        <div class="order-card">
                            <div class="order-header">
                                <div>
                                    <div class="order-id">Order #${order.id}</div>
                                    <small class="text-muted">${new Date(order.createdAt).toLocaleDateString()} at ${new Date(order.createdAt).toLocaleTimeString()}</small>
                                </div>
                                <div class="text-end">
                                    <span class="badge ${getStatusBadge(order.status)} badge-custom">${order.status}</span>
                                    <div class="mt-2"><h5 class="text-warning">${order.total.toFixed(2)}</h5></div>
                                </div>
                            </div>
                            
                            <div class="mb-3">
                                <strong>Items:</strong>
                                <ul class="mt-2">
                                    ${order.orderItems.map(item => `
                                        <li>${item.menuItemName} x${item.quantity} - ${item.subtotal.toFixed(2)}</li>
                                    `).join('')}
                                </ul>
                            </div>
                            
                            <div class="row">
                                <div class="col-md-6">
                                    <small class="text-muted">Order Type:</small> <strong>${order.orderType}</strong><br>
                                    ${order.deliveryAddress ? `<small class="text-muted">Address:</small> ${order.deliveryAddress}<br>` : ''}
                                    <small class="text-muted">Payment:</small> <strong>${order.paymentMethod}</strong>
                                </div>
                                <div class="col-md-6 text-end">
                                    <small class="text-muted">Subtotal:</small> ${order.subtotal.toFixed(2)}<br>
                                    <small class="text-muted">Tax:</small> ${order.tax.toFixed(2)}<br>
                                    ${order.discount > 0 ? `<small class="text-success">Discount:</small> -${order.discount.toFixed(2)}<br>` : ''}
                                </div>
                            </div>
                        </div>
                    `).join('')}
                </div>
            `;
            
            document.getElementById('dynamicContent').innerHTML = html;
        }

        // Profile Display
        function displayProfile() {
            const html = `
                <div class="content-section">
                    <h5 class="section-title">
                        <i class="fas fa-user"></i> My Profile
                    </h5>
                    
                    <div class="text-center mb-4">
                        <img src="https://via.placeholder.com/150" alt="Profile" class="rounded-circle mb-3" style="width: 150px; height: 150px;">
                        <h4 id="profileName">Customer Name</h4>
                        <p class="text-muted" id="profileEmail">customer@example.com</p>
                    </div>
                    
                    <div class="alert alert-info">
                        <i class="fas fa-info-circle me-2"></i>
                        Profile editing feature will be available soon
                    </div>
                </div>
            `;
            
            document.getElementById('dynamicContent').innerHTML = html;
        }

        // Cart Actions
        async function addToCart(productId) {
            try {
                const response = await fetch(`${API_BASE}/customer/cart/add`, {
                    method: 'POST',
                    headers: {
                        'Authorization': `Bearer ${authToken}`,
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({ productId: productId, quantity: 1 })
                });

                if (response.ok) {
                    const result = await response.json();
                    if (result.success) {
                        showNotification(' Added to cart!', 'success');
                        await updateCartCount();
                    }
                } else {
                    showNotification(' Failed to add to cart', 'error');
                }
            } catch (error) {
                console.error('Error adding to cart:', error);
                showNotification(' Error adding to cart', 'error');
            }
        }

        async function updateCartQuantity(cartItemId, newQuantity) {
            if (newQuantity < 1) {
                removeFromCart(cartItemId);
                return;
            }

            try {
                const response = await fetch(`${API_BASE}/customer/cart/update/${cartItemId}`, {
                    method: 'PUT',
                    headers: {
                        'Authorization': `Bearer ${authToken}`,
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({ quantity: newQuantity })
                });

                if (response.ok) {
                    showNotification(' Cart updated', 'success');
                    loadSectionContent('cart');
                    await updateCartCount();
                }
            } catch (error) {
                console.error('Error updating cart:', error);
            }
        }

        async function removeFromCart(cartItemId) {
            if (!confirm('Remove this item from cart?')) return;

            try {
                const response = await fetch(`${API_BASE}/customer/cart/remove/${cartItemId}`, {
                    method: 'DELETE',
                    headers: {
                        'Authorization': `Bearer ${authToken}`,
                        'Content-Type': 'application/json'
                    }
                });

                if (response.ok) {
                    showNotification(' Removed from cart', 'success');
                    loadSectionContent('cart');
                    await updateCartCount();
                }
            } catch (error) {
                console.error('Error removing from cart:', error);
            }
        }

async function proceedToCheckout() {
    const token = localStorage.getItem('jwt_token');
    if (!token) {
        alert('Please login first');
        window.location.href = 'login.html';
        return;
    }

    // Validate form
    const form = document.getElementById('checkoutForm');
    if (!form.checkValidity()) {
        form.reportValidity();
        return;
    }

    try {
        // Get form values
        const customerName = document.getElementById('customerName').value.trim();
        const phoneNumber = document.getElementById('phoneNumber').value.trim();
        const orderType = parseInt(document.getElementById('orderType').value,10);
        const paymentMethod = parseInt(document.getElementById('paymentMethod').value, 10);
        const deliveryAddress = orderType === 1 ? document.getElementById('deliveryAddress').value.trim() : null;
        const notes = document.getElementById('orderNotes').value.trim();

        // Validate delivery address
        if (orderType === 1 && !deliveryAddress) {
            alert('Please enter delivery address');
            return;
        }

        // Save to localStorage for next time
        localStorage.setItem('user_name', customerName);
        localStorage.setItem('user_phone', phoneNumber);

        const orderData = {
            customerName,
            phoneNumber,
            orderType,
            deliveryAddress,
            paymentMethod,
            notes
        };

        console.log("Sending order:", orderData);

        const response = await fetch(`${API_BASE}/customer/orders/place-order`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`
            },
            body: JSON.stringify(orderData)
        });

        const text = await response.text();
        console.log("Raw response:", text);

        let result;
        try {
            result = JSON.parse(text);
        } catch {
            throw new Error("Invalid response from server");
        }

        if (response.ok && result.success) {
            showNotification('Order placed successfully!', 'success');
            // Clear cart UI
            document.getElementById('cartCount').textContent = '0';
            document.getElementById('cartBadge').textContent = '0';
            // Redirect
            setTimeout(() => {
                window.location.href = 'order-success.html';
            }, 1000);
        } else {
            const errorMsg = result.message || "Failed to place order";
            showNotification(errorMsg, 'error');
            if (result.error) console.error("Server error:", result.error);
        }

    } catch (err) {
        console.error("Checkout error:", err);
        showNotification('Network error. Please try again.', 'error');
    }
}

        // Wishlist Actions
        async function addToWishlist(productId) {
            try {
                const response = await fetch(`${API_BASE}/customer/wishlists/add`, {
                    method: 'POST',
                    headers: {
                        'Authorization': `Bearer ${authToken}`,
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({ productId: productId })
                });

                if (response.ok) {
                    const result = await response.json();
                    if (result.success) {
                        showNotification(' Added to wishlist!', 'success');
                        await updateWishlistCount();
                    } else {
                        showNotification(result.message || ' Failed to add', 'error');
                    }
                }
            } catch (error) {
                console.error('Error adding to wishlist:', error);
            }
        }

        async function removeFromWishlist(wishlistItemId) {
            if (!confirm('Remove from wishlist?')) return;

            try {
                const response = await fetch(`${API_BASE}/customer/wishlists/remove/${wishlistItemId}`, {
                    method: 'DELETE',
                    headers: {
                        'Authorization': `Bearer ${authToken}`,
                        'Content-Type': 'application/json'
                    }
                });

                if (response.ok) {
                    showNotification(' Removed from wishlist', 'success');
                    loadSectionContent('wishlist');
                    await updateWishlistCount();
                }
            } catch (error) {
                console.error('Error removing from wishlist:', error);
            }
        }

        async function moveToCart(productId) {
            await addToCart(productId);
        }

        // Notifications
        function showNotification(message, type = 'info') {
            const colors = {
                success: 'alert-success',
                error: 'alert-danger',
                info: 'alert-info',
                warning: 'alert-warning'
            };

            const notification = document.createElement('div');
            notification.className = `alert ${colors[type]} alert-dismissible fade show`;
            notification.style.position = 'fixed';
            notification.style.top = '80px';
            notification.style.right = '20px';
            notification.style.zIndex = '9999';
            notification.style.minWidth = '300px';
            notification.innerHTML = `
                ${message}
                <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
            `;

            document.body.appendChild(notification);

            setTimeout(() => {
                notification.remove();
            }, 3000);
        }

        // Logout
        function logout() {
            if (confirm('Are you sure you want to logout?')) {
                localStorage.removeItem('jwt_token');
                localStorage.removeItem('user_roles');
                window.location.href = 'Account.html';
            }
        }
    
