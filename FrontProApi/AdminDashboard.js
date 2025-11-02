        // API Configuration
        const API_ENDPOINTS = [
            'https://localhost:7201/api'
        ];

        let API_BASE = '';
        let authToken = localStorage.getItem('jwt_token');
        let isAPIConnected = false;

        // Initialize Dashboard
        document.addEventListener('DOMContentLoaded', async function () {
            // Check if user is logged in
            if (!authToken) {
                console.error(' No authentication token found');
                alert('Please login first!');
                window.location.href = 'login.html';
                return;
            }

            // Try to find working API endpoint
            await findWorkingAPIEndpoint();

            // Check if API is connected
            if (!isAPIConnected) {
                showAPIError();
                return;
            }

            // Load dashboard data
            loadDashboardStats();
            loadRecentOrders();
        });

        // API Connection Functions
        async function findWorkingAPIEndpoint() {
            for (const endpoint of API_ENDPOINTS) {
                try {
                    console.log(`Testing: ${endpoint}/Admin/dashboard/stats`);

                    const response = await fetch(`${endpoint}/Admin/dashboard/stats`, {
                        method: 'GET',
                        headers: {
                            'Authorization': `Bearer ${authToken}`,
                            'Content-Type': 'application/json'
                        }
                    });

                    console.log(`Status: ${response.status}`);

                    if (response.ok) {
                        const result = await response.json();
                        if (result.success) {
                            API_BASE = endpoint;
                            isAPIConnected = true;
                            showAPIConnected();
                            return;
                        }
                    } else if (response.status === 403) {
                        alert('Access denied: You are not an Admin! Check your roles.');
                        logout();
                        return;
                    }
                } catch (error) {
                    console.error(`Failed: ${endpoint}`, error);
                }
            }
            showAPIError();
        }
        function showAPIError() {
            document.getElementById('apiStatus').innerHTML = `
            <strong> API Connection Failed</strong> - Cannot connect to backend
            <br><small>Make sure your API is running on one of these ports: 7201, 5064, 32080</small>
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        `;
            document.getElementById('apiStatus').className = 'alert alert-danger alert-dismissible fade show';
        }

        function showDemoMode() {
            document.getElementById('apiStatus').innerHTML = `
            <strong>🔌 Demo Mode</strong> - API not connected, using sample data
            <br><small>Start your API server to see real data from database</small>
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        `;
            document.getElementById('apiStatus').className = 'alert alert-warning alert-dismissible fade show';
        }

        function showAPIConnected() {
            document.getElementById('apiStatus').innerHTML = `
            <strong>API Connected</strong> - Using real data from ${API_BASE}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        `;
            document.getElementById('apiStatus').className = 'alert alert-success alert-dismissible fade show';
        }

        // Navigation Functions
        function showSection(section) {
            // Update active nav link
            document.querySelectorAll('.nav-link').forEach(link => link.classList.remove('active'));
            event.currentTarget.classList.add('active');

            showLoading();

            // Update section title
            const titles = {
                'dashboard': 'Dashboard Overview',
                'categories': 'Categories Management',
                'products': 'Products Management',
                'orders': 'Orders Management',
                'users': 'Users Management',
                'customers': 'Customers Management',
                'wishlists': 'Wishlists Management'
            };
            document.getElementById('sectionTitle').textContent = titles[section] || 'Management';

            // Load section content
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

        // Dashboard Data Loading
        async function loadDashboardStats() {
            if (!isAPIConnected) {
                console.error(' Cannot load dashboard stats - API not connected');
                return;
            }

            try {
                const response = await fetch(`${API_BASE}/Admin/dashboard/stats`, {
                    headers: {
                        'Authorization': `Bearer ${authToken}`,
                        'Content-Type': 'application/json'
                    }
                });

                if (response.ok) {
                    const result = await response.json();
                    if (result.success) {
                        const stats = result.data;
                        document.getElementById('categoriesCount').textContent = stats.totalCategories;
                        document.getElementById('productsCount').textContent = stats.totalProducts;
                        document.getElementById('ordersCount').textContent = stats.totalOrders;
                        document.getElementById('usersCount').textContent = stats.totalUsers;
                        console.log(' Dashboard stats loaded successfully');
                        return;
                    }
                }

                throw new Error(`Failed to load dashboard stats: ${response.status}`);
            } catch (error) {
                console.error(' Error loading dashboard stats:', error);
                document.getElementById('categoriesCount').textContent = 'Error';
                document.getElementById('productsCount').textContent = 'Error';
                document.getElementById('ordersCount').textContent = 'Error';
                document.getElementById('usersCount').textContent = 'Error';
            }
        }

        async function loadRecentOrders() {
            try {
                const response = await fetch(`${API_BASE}/Admin/orders?page=1&pageSize=5`, {
                    headers: {
                        'Authorization': `Bearer ${authToken}`,
                        'Content-Type': 'application/json'
                    }
                });

                if (response.ok) {
                    const result = await response.json();
                    if (result.success) {
                        const orders = result.data;
                        const tableBody = document.getElementById('recentOrdersTable');
                        tableBody.innerHTML = orders.map(order => `
                        <tr>
                            <td>#${order.id}</td>
                            <td>${order.customerName}</td>
                            <td>${order.total.toFixed(2)}</td>
                            <td><span class="badge ${getStatusBadgeClass(order.status)}">${order.status}</span></td>
                            <td>${new Date(order.createdAt).toLocaleDateString()}</td>
                        </tr>
                    `).join('');
                    }
                }
            } catch (error) {
                console.error('Error loading recent orders:', error);
            }
        }

        function getStatusBadgeClass(status) {
            const classes = {
                'Pending': 'bg-warning',
                'Preparing': 'bg-info',
                'Ready': 'bg-primary',
                'Delivered': 'bg-success',
                'Canceled': 'bg-danger'
            };
            return classes[status] || 'bg-secondary';
        }

        async function loadSectionContent(section) {
            if (!isAPIConnected) {
                useMockDataForSection(section);
                return;
            }

            try {
                const response = await fetch(`${API_BASE}/admin/${section}`, {
                    headers: {
                        'Authorization': `Bearer ${authToken}`,
                        'Content-Type': 'application/json'
                    }
                });

                if (response.ok) {
                    const result = await response.json();
                    if (result.success) {
                        displaySectionData(section, result.data);
                        return;
                    }
                }
            } catch (error) {
                console.log(`API failed for ${section}, using mock data`);
            }

            useMockDataForSection(section);
        }

        
        // Section Data Display
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
                // Build the correct endpoint
                let endpoint = '';
                switch (section) {
                    case 'categories':
                        endpoint = `${API_BASE}/admin/categories`;
                        break;
                    case 'products':
                        endpoint = `${API_BASE}/admin/products`;
                        break;
                    case 'orders':
                        endpoint = `${API_BASE}/admin/orders`;
                        break;
                    case 'users':
                        endpoint = `${API_BASE}/admin/users`;
                        break;
                    case 'customers':
                        endpoint = `${API_BASE}/admin/users/customers`;
                        break;
                    case 'wishlists':
                        endpoint = `${API_BASE}/admin/wishlists`;
                        break;
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

                console.log(` Response status for ${section}:`, response.status);

                if (response.ok) {
                    const result = await response.json();
                    console.log(` Data for ${section}:`, result);

                    if (result.success && result.data) {
                        if (Array.isArray(result.data) && result.data.length === 0) {
                            document.getElementById('dynamicContent').innerHTML = `
                            <div class="alert alert-info text-center">
                                <i class="fas fa-info-circle fa-2x mb-3"></i>
                                <h5>No ${section} Found</h5>
                                <p>The database is empty for this section. Add some data to get started!</p>
                            </div>
                        `;
                        } else {
                            displaySectionData(section, result.data);
                            console.log(` ${section} loaded successfully`);
                        }
                        return;
                    }
                }

                throw new Error(`API request failed with status ${response.status}`);
            } catch (error) {
                console.error(` Error loading ${section}:`, error);
                document.getElementById('dynamicContent').innerHTML = `
                <div class="alert alert-danger">
                    <h5><i class="fas fa-exclamation-triangle me-2"></i>Error Loading ${section}</h5>
                    <p><strong>Error:</strong> ${error.message}</p>
                    <hr>
                    <p><strong>Troubleshooting:</strong></p>
                    <ul>
                        <li>Check if API is running at: <code>${API_BASE}</code></li>
                        <li>Verify your authentication token is valid</li>
                        <li>Ensure you have admin permissions</li>
                        <li>Check browser console for detailed errors</li>
                    </ul>
                    <button class="btn btn-primary mt-2" onclick="loadSectionContent('${section}')">
                        <i class="fas fa-sync-alt me-2"></i>Retry
                    </button>
                </div>
            `;
            }
        }

        function displaySectionData(section, data) {
            let content = '';

            switch (section) {
                case 'categories':
                    content = generateCategoriesTable(data);
                    break;
                case 'products':
                    content = generateProductsTable(data);
                    break;
                case 'orders':
                    content = generateOrdersTable(data);
                    break;
                case 'users':
                    content = generateUsersTable(data);
                    break;
                case 'customers':
                    content = generateCustomersTable(data);
                    break;
                case 'wishlists':
                    content = generateWishlistsTable(data);
                    break;
                default:
                    content = '<div class="alert alert-info">Section not implemented yet</div>';
            }

            document.getElementById('dynamicContent').innerHTML = content;
        }

        // Table Generation Functions

        // Generate Categories Table
        function generateCategoriesTable(categories) {
            return `
            <div class="content-section">
                <div class="d-flex justify-content-between align-items-center mb-4">
                    <h5>Categories Management</h5>
                    <button class="btn btn-custom" onclick="addNewCategory()">
                        <i class="fas fa-plus me-2"></i>Add Category
                    </button>
                </div>
                <div class="table-responsive">
                    <table class="table table-hover">
                        <thead>
                            <tr>
                                <th>ID</th>
                                <th>Image</th>
                                <th>Name</th>
                                <th>Description</th>
                                <th>Items Count</th>
                                <th>Status</th>
                                <th>Actions</th>
                            </tr>
                        </thead>
                        <tbody>
                            ${categories.map(cat => `
                                <tr>
                                    <td>${cat.id}</td>
                                    <td>
                                        <img src="${cat.imageUrl || 'https://via.placeholder.com/50'}" 
                                             alt="${cat.name}" 
                                             style="width: 50px; height: 50px; object-fit: cover; border-radius: 8px;">
                                    </td>
                                    <td><strong>${cat.name}</strong></td>
                                    <td>${cat.description}</td>
                                    <td><span class="badge bg-info">${cat.menuItemsCount || 0} Items</span></td>
                                    <td>
                                        <span class="badge ${cat.isActive ? 'bg-success' : 'bg-danger'}">
                                            ${cat.isActive ? 'Active' : 'Inactive'}
                                        </span>
                                    </td>
                                    <td class="row_actions">
                                        <button class="btn btn-sm btn-warning me-1" onclick="editCategory(${cat.id})">
                                            <i class="fas fa-edit"></i>
                                        </button>
                                        <button class="btn btn-sm btn-danger" onclick="deleteCategory(${cat.id})">
                                            <i class="fas fa-trash"></i>
                                        </button>
                                    </td>
                                </tr>
                            `).join('')}
                        </tbody>
                    </table>
                </div>
            </div>
        `;
        }

        // Generate Products Table
        function generateProductsTable(products) {
            return `
            <div class="content-section">
                <div class="d-flex justify-content-between align-items-center mb-4">
                    <h5>Products Management</h5>
                    <button class="btn btn-custom" onclick="addNewProduct()">
                        <i class="fas fa-plus me-2"></i>Add Product
                    </button>
                </div>
                <div class="table-responsive">
                    <table class="table table-hover">
                        <thead>
                            <tr>
                                <th>ID</th>
                                <th>Image</th>
                                <th>Name</th>
                                <th>Category</th>
                                <th>Price</th>
                                <th>Prep Time</th>
                                <th>Status</th>
                                <th>Actions</th>
                            </tr>
                        </thead>
                        <tbody>
                            ${products.map(product => `
                                <tr>
                                    <td>${product.id}</td>
                                    <td>
                                        <img src="${product.imageUrl || 'https://via.placeholder.com/50'}" 
                                             alt="${product.name}" 
                                             style="width: 50px; height: 50px; object-fit: cover; border-radius: 8px;">
                                    </td>
                                    <td>
                                        <strong>${product.name}</strong><br>
                                        <small class="text-muted">${product.description}</small>
                                    </td>
                                    <td><span class="badge bg-secondary">${product.categoryName || 'N/A'}</span></td>
                                    <td><strong>$${product.price.toFixed(2)}</strong></td>
                                    <td>${product.preparationTime} min</td>
                                    <td>
                                        <span class="badge ${product.isAvailable ? 'bg-success' : 'bg-danger'}">
                                            ${product.isAvailable ? 'Available' : 'Unavailable'}
                                        </span>
                                    </td>
                                    <td class="row_actions">
                                        <button class="btn btn-sm btn-warning me-1" onclick="editProduct(${product.id})">
                                            <i class="fas fa-edit"></i>
                                        </button>
                                        <button class="btn btn-sm btn-danger" onclick="deleteProduct(${product.id})">
                                            <i class="fas fa-trash"></i>
                                        </button>
                                    </td>
                                </tr>
                            `).join('')}
                        </tbody>
                    </table>
                </div>
            </div>
        `;
        }

        // Generate Orders Table
        function generateOrdersTable(orders) {
            return `
            <div class="content-section">
                <div class="d-flex justify-content-between align-items-center mb-4">
                    <h5>Orders Management</h5>
                </div>
                <div class="table-responsive">
                    <table class="table table-hover">
                        <thead>
                            <tr>
                                <th>Order ID</th>
                                <th>Customer</th>
                                <th>Total</th>
                                <th>Status</th>
                                <th>Date</th>
                                <th>Actions</th>
                            </tr>
                        </thead>
                        <tbody>
                            ${orders.map(order => `
                                <tr>
                                    <td>#${order.id}</td>
                                    <td>${order.customerName}</td>
                                    <td>$${order.total.toFixed(2)}</td>
                                    <td><span class="badge ${getStatusBadgeClass(order.status)}">${order.status}</span></td>
                                    <td>${new Date(order.createdAt).toLocaleDateString()}</td>
                                    <td class="row_actions">
                                        <button class="btn btn-sm btn-info me-1" onclick="viewOrder(${order.id})">
                                            <i class="fas fa-eye"></i>
                                        </button>
                                        <button class="btn btn-sm btn-warning" onclick="updateOrderStatus(${order.id})">
                                            <i class="fas fa-edit"></i>
                                        </button>
                                    </td>
                                </tr>
                            `).join('')}
                        </tbody>
                    </table>
                </div>
            </div>
        `;
        }

        // Generate Users Table
        function generateUsersTable(users) {
            return `
            <div class="content-section">
                <div class="d-flex justify-content-between align-items-center mb-4">
                    <h5>Users Management</h5>
                    <button class="btn btn-custom" onclick="addNewUser()">
                        <i class="fas fa-plus me-2"></i>Add User
                    </button>
                </div>
                <div class="table-responsive">
                    <table class="table table-hover">
                        <thead>
                            <tr>
                                <th>User ID</th>
                                <th>Image</th>
                                <th>Username</th>
                                <th>Email</th>
                                <th>Role</th>
                                <th>Actions</th>
                            </tr>
                        </thead>
                        <tbody>
                            ${users.map(user => `
                                <tr class="row_tr">
                                    <td>${user.userId}</td>
                                    <td>
                                        <img src="${user.imageUrl || 'https://via.placeholder.com/40'}" 
                                             alt="${user.userName}" 
                                             class="user-avatar">
                                    </td>
                                    <td><strong>${user.userName}</strong></td>
                                    <td>${user.email}</td>
                                    <td>
                                        <span class="badge ${user.roles && user.roles.includes('Admin') ? 'bg-danger' : 'bg-primary'}">
                                            ${user.roles ? user.roles[0] : 'User'}
                                        </span>
                                    </td>
                                    <td class="row_actions">
                                        <button class="btn btn-sm btn-warning me-1" onclick="editUser('${user.userId}')">
                                            <i class="fas fa-edit"></i>
                                        </button>
                                        <button class="btn btn-sm btn-danger" onclick="deleteUser('${user.userId}')">
                                            <i class="fas fa-trash"></i>
                                        </button>
                                    </td>
                                </tr>
                            `).join('')}
                        </tbody>
                    </table>
                </div>
            </div>
        `;
        }

        // Generate Customers Table
        function generateCustomersTable(customers) {
            fetch(`${API_BASE}/admin/orders`, { headers: { 'Authorization': `Bearer ${authToken}` } })
            .then(r => r.json())
            .then(ordersRes => {
                const orders = ordersRes.success ? ordersRes.data : [];
                const tableBody = customers.map(cust => {
                    const orderCount = orders.filter(o => o.userId === cust.userId).length;
                    return `
                        <tr>
                            <td><small>${cust.userId.substring(0, 8)}...</small></td>
                            <td><img src="${cust.imageUrl || '/images/users/default-avatar.jpg'}" class="user-avatar"></td>
                            <td><strong>${cust.userName}</strong></td>
                            <td>${cust.email}</td>
                            <td>${cust.phoneNumber || 'N/A'}</td>
                            <td><span class="badge bg-info">${orderCount}</span></td>
                            <td class="row_actions">
                                <button class="btn btn-sm btn-info me-1" onclick="viewCustomer('${cust.userId}')">
                                    <i class="fas fa-eye"></i>
                                </button>
                                <button class="btn btn-sm btn-warning me-1" onclick="editCustomer('${cust.userId}')">
                                    <i class="fas fa-edit"></i>
                                </button>
                                <button class="btn btn-sm btn-danger" onclick="deleteCustomer('${cust.userId}')">
                                    <i class="fas fa-trash"></i>
                                </button>
                            </td>
                        </tr>
                    `;
                }).join('');

                document.getElementById('dynamicContent').innerHTML = `
                    <div class="content-section">
                        <div class="d-flex justify-content-between align-items-center mb-4">
                            <h5>Customers Management</h5>
                            <button class="btn btn-custom" onclick="addNewUser()">
                                <i class="fas fa-plus me-2"></i>Add Customer
                            </button>
                        </div>
                        <div class="table-responsive">
                            <table class="table table-hover">
                                <thead>
                                    <tr>
                                        <th>ID</th>
                                        <th>Image</th>
                                        <th>Name</th>
                                        <th>Email</th>
                                        <th>Phone</th>
                                        <th>Orders</th>
                                        <th>Actions</th>
                                    </tr>
                                </thead>
                                <tbody>${tableBody}</tbody>
                            </table>
                        </div>
                    </div>
                `;
            });
        }        
        // Generate Wishlists Table
        function generateWishlistsTable(wishlists) {
            return `
            <div class="content-section">
                <div class="d-flex justify-content-between align-items-center mb-4">
                    <h5>Wishlists Management</h5>
                </div>
                <div class="table-responsive">
                    <table class="table table-hover">
                        <thead>
                            <tr>
                                <th>ID</th>
                                <th>Customer</th>
                                <th>Email</th>
                                <th>Items Count</th>
                                <th>Total Value</th>
                                <th>Actions</th>
                            </tr>
                        </thead>
                        <tbody>
                            ${wishlists.map(wishlist => `
                                <tr>
                                    <td>#${wishlist.id}</td>
                                    <td><strong>${wishlist.userName}</strong></td>
                                    <td>${wishlist.userEmail}</td>
                                    <td><span class="badge bg-info">${wishlist.itemsCount} Items</span></td>
                                    <td>$${wishlist.totalEstimatedPrice.toFixed(2)}</td>
                                    <td class="row_actions">
                                        <button class="btn btn-sm btn-info me-1" onclick="viewWishlist(${wishlist.id})">
                                            <i class="fas fa-eye"></i>
                                        </button>
                                        <button class="btn btn-sm btn-danger" onclick="deleteWishlist(${wishlist.id})">
                                            <i class="fas fa-trash"></i>
                                        </button>
                                    </td>
                                </tr>
                            `).join('')}
                        </tbody>
                    </table>
                </div>
            </div>
        `;
        }
    // Action Functions - Categories
    let currentCategoryId = null;

    function addNewCategory() {
        currentCategoryId = null;
        document.getElementById('modalTitle').textContent = 'Add New Category';
        document.getElementById('categoryForm').reset();
        document.getElementById('categoryId').value = '';
        document.getElementById('currentImage').style.display = 'none';
        new bootstrap.Modal(document.getElementById('categoryModal')).show();
    }

    function editCategory(id) {
        currentCategoryId = id;
        document.getElementById('modalTitle').textContent = 'Edit Category';

        fetch(`${API_BASE}/admin/categories/${id}`, {
            headers: { 'Authorization': `Bearer ${authToken}` }
        })
        .then(res => res.json())
        .then(result => {
            if (result.success) {
                const cat = result.data;
                document.getElementById('categoryId').value = cat.id;
                document.getElementById('categoryName').value = cat.name;
                document.getElementById('categoryDescription').value = cat.description || '';
                document.getElementById('categoryIsActive').checked = cat.isActive;

                const img = document.getElementById('currentImage');
                if (cat.imageUrl && !cat.imageUrl.includes('default')) {
                    img.src = cat.imageUrl;
                    img.style.display = 'block';
                } else {
                    img.style.display = 'none';
                }
                new bootstrap.Modal(document.getElementById('categoryModal')).show();
            }
        })
        .catch(err => alert('Failed to load category'));
    }

    function deleteCategory(id) {
        if (!confirm('Are you sure you want to delete this category? All products will be soft-deleted.')) return;

        fetch(`${API_BASE}/admin/categories/${id}`, {
            method: 'DELETE',
            headers: { 'Authorization': `Bearer ${authToken}` }
        })
        .then(res => res.json())
        .then(result => {
            if (result.success) {
                alert('Category deleted successfully');
                loadSectionContent('categories');
            } else {
                alert(result.message || 'Failed to delete');
            }
        })
        .catch(err => alert('Error: ' + err));
    }

    // Handle form submit
    document.getElementById('categoryForm').addEventListener('submit', function(e) {
        e.preventDefault();

        const formData = new FormData();
        formData.append('Name', document.getElementById('categoryName').value.trim());
        formData.append('Description', document.getElementById('categoryDescription').value);
        formData.append('IsActive', document.getElementById('categoryIsActive').checked);

        const imageFile = document.getElementById('categoryImage').files[0];
        if (imageFile) formData.append('ImageFile', imageFile);

        const id = document.getElementById('categoryId').value;
        const method = id ? 'PUT' : 'POST';
        const url = id ? `${API_BASE}/admin/categories/${id}` : `${API_BASE}/admin/categories`;

        fetch(url, {
            method: method,
            headers: { 'Authorization': `Bearer ${authToken}` },
            body: formData
        })
        .then(res => res.json())
        .then(result => {
            if (result.success) {
                alert(id ? 'Category updated!' : 'Category added!');
                bootstrap.Modal.getInstance(document.getElementById('categoryModal')).hide();
                loadSectionContent('categories');
            } else {
                alert(result.message || 'Operation failed');
            }
        })
        .catch(err => alert('Error: ' + err));
    });

        // Action Functions - Products
        let currentProductId = null;

        function addNewProduct() {
            currentProductId = null;
            document.getElementById('productModalTitle').textContent = 'Add New Product';
            document.getElementById('productForm').reset();
            document.getElementById('productId').value = '';
            document.getElementById('productCurrentImage').style.display = 'none';
            loadCategoriesForProduct(); // Load categories dropdown
            new bootstrap.Modal(document.getElementById('productModal')).show();
        }

        function editProduct(id) {
            currentProductId = id;
            document.getElementById('productModalTitle').textContent = 'Edit Product';

            Promise.all([
                fetch(`${API_BASE}/admin/categories`, { headers: { 'Authorization': `Bearer ${authToken}` } }).then(r => r.json()),
                fetch(`${API_BASE}/admin/products/${id}`, { headers: { 'Authorization': `Bearer ${authToken}` } }).then(r => r.json())
            ])
            .then(([catRes, prodRes]) => {
                if (catRes.success && prodRes.success) {
                    const categories = catRes.data;
                    const product = prodRes.data;

                    // Populate categories
                    const select = document.getElementById('productCategoryId');
                    select.innerHTML = categories.map(c => 
                        `<option value="${c.id}" ${c.id === product.categoryId ? 'selected' : ''}>${c.name}</option>`
                    ).join('');

                    // Populate form
                    document.getElementById('productId').value = product.id;
                    document.getElementById('productName').value = product.name;
                    document.getElementById('productDescription').value = product.description || '';
                    document.getElementById('productPrice').value = product.price;
                    document.getElementById('productPrepTime').value = product.preparationTime;
                    document.getElementById('productIsAvailable').checked = product.isAvailable;

                    const img = document.getElementById('productCurrentImage');
                    if (product.imageUrl && !product.imageUrl.includes('default')) {
                        img.src = product.imageUrl;
                        img.style.display = 'block';
                    } else {
                        img.style.display = 'none';
                    }

                    new bootstrap.Modal(document.getElementById('productModal')).show();
                }
            })
            .catch(err => alert('Failed to load data: ' + err));
        }

        function deleteProduct(id) {
            if (!confirm('Are you sure you want to delete this product?')) return;

            fetch(`${API_BASE}/admin/products/${id}`, {
                method: 'DELETE',
                headers: { 'Authorization': `Bearer ${authToken}` }
            })
            .then(res => res.json())
            .then(result => {
                if (result.success) {
                    alert('Product deleted successfully');
                    loadSectionContent('products');
                } else {
                    alert(result.message || 'Failed to delete');
                }
            })
            .catch(err => alert('Error: ' + err));
        }

        // Load categories into dropdown
        async function loadCategoriesForProduct() {
            try {
                const res = await fetch(`${API_BASE}/admin/categories`, {
                    headers: { 'Authorization': `Bearer ${authToken}` }
                });
                const result = await res.json();
                if (result.success) {
                    const select = document.getElementById('productCategoryId');
                    select.innerHTML = result.data.map(c => 
                        `<option value="${c.id}">${c.name}</option>`
                    ).join('');
                }
            } catch (err) {
                console.error('Failed to load categories', err);
            }
        }

        // Handle product form submit
        document.getElementById('productForm').addEventListener('submit', function(e) {
            e.preventDefault();

            const formData = new FormData();
            formData.append('Name', document.getElementById('productName').value.trim());
            formData.append('Description', document.getElementById('productDescription').value);
            formData.append('Price', document.getElementById('productPrice').value);
            formData.append('CategoryId', document.getElementById('productCategoryId').value);
            formData.append('PreparationTime', document.getElementById('productPrepTime').value);
            formData.append('IsAvailable', document.getElementById('productIsAvailable').checked);

            const imageFile = document.getElementById('productImage').files[0];
            if (imageFile) formData.append('ImageFile', imageFile);

            const id = document.getElementById('productId').value;
            const method = id ? 'PUT' : 'POST';
            const url = id ? `${API_BASE}/admin/products/${id}` : `${API_BASE}/admin/products`;

            fetch(url, {
                method: method,
                headers: { 'Authorization': `Bearer ${authToken}` },
                body: formData
            })
            .then(res => res.json())
            .then(result => {
                if (result.success) {
                    alert(id ? 'Product updated!' : 'Product added!');
                    bootstrap.Modal.getInstance(document.getElementById('productModal')).hide();
                    loadSectionContent('products');
                } else {
                    alert(result.message || 'Operation failed');
                }
            })
            .catch(err => alert('Error: ' + err));
        });

        // Action Functions - Orders
        let currentOrderId = null;

        function viewOrder(id) {
            fetch(`${API_BASE}/admin/orders/${id}`, {
                headers: { 'Authorization': `Bearer ${authToken}` }
            })
            .then(res => res.json())
            .then(result => {
                if (result.success) {
                    const o = result.data;
                    const itemsHtml = o.orderItems.map(item => `
                        <tr>
                            <td>${item.menuItemName}</td>
                            <td>${item.quantity}</td>
                            <td>$${item.price.toFixed(2)}</td>
                            <td>$${item.subtotal.toFixed(2)}</td>
                            <td><small>${item.specialInstructions || '-'}</small></td>
                        </tr>
                    `).join('');

                    document.getElementById('orderDetailsBody').innerHTML = `
                        <div class="row">
                            <div class="col-md-6">
                                <p><strong>Order ID:</strong> #${o.id}</p>
                                <p><strong>Customer:</strong> ${o.customerName}</p>
                                <p><strong>Phone:</strong> ${o.phoneNumber}</p>
                                <p><strong>Type:</strong> ${o.orderType}</p>
                                ${o.orderType === 'Delivery' ? `<p><strong>Address:</strong> ${o.deliveryAddress}</p>` : ''}
                            </div>
                            <div class="col-md-6">
                                <p><strong>Status:</strong> <span class="badge ${getStatusBadgeClass(o.status)}">${o.status}</span></p>
                                <p><strong>Payment:</strong> ${o.paymentMethod}</p>
                                <p><strong>Est. Time:</strong> ${o.estimatedDeliveryTime || 'N/A'}</p>
                                <p><strong>Date:</strong> ${new Date(o.createdAt).toLocaleString()}</p>
                            </div>
                        </div>
                        <hr>
                        <h6>Order Items</h6>
                        <table class="table table-sm">
                            <thead>
                                <tr>
                                    <th>Item</th>
                                    <th>Qty</th>
                                    <th>Price</th>
                                    <th>Subtotal</th>
                                    <th>Notes</th>
                                </tr>
                            </thead>
                            <tbody>${itemsHtml}</tbody>
                        </table>
                        <hr>
                        <div class="row text-end">
                            <div class="col">
                                <p><strong>Subtotal:</strong> $${o.subtotal.toFixed(2)}</p>
                                <p><strong>Tax:</strong> $${o.tax.toFixed(2)}</p>
                                <p><strong>Discount:</strong> $${o.discount.toFixed(2)}</p>
                                <p class="h5"><strong>Total: $${o.total.toFixed(2)}</strong></p>
                            </div>
                        </div>
                        ${o.notes ? `<hr><p><strong>Notes:</strong><br><small>${o.notes.replace(/\n/g, '<br>')}</small></p>` : ''}
                    `;
                    new bootstrap.Modal(document.getElementById('orderDetailsModal')).show();
                } else {
                    alert(result.message);
                }
            })
            .catch(err => alert('Error loading order'));
        }

        function updateOrderStatus(id) {
            currentOrderId = id;
            document.getElementById('orderIdForStatus').value = id;
            document.getElementById('orderStatusForm').reset();

            // Load current status
            fetch(`${API_BASE}/admin/orders/${id}`, {
                headers: { 'Authorization': `Bearer ${authToken}` }
            })
            .then(res => res.json())
            .then(result => {
                if (result.success) {
                    document.getElementById('newOrderStatus').value = result.data.status;
                    new bootstrap.Modal(document.getElementById('orderStatusModal')).show();
                }
            });
        }

        // Handle status update
        document.getElementById('orderStatusForm').addEventListener('submit', function(e) {
            e.preventDefault();

            const payload = {
                newStatus: document.getElementById('newOrderStatus').value,
                notes: document.getElementById('statusNotes').value.trim()
            };

            fetch(`${API_BASE}/admin/orders/${currentOrderId}/status`, {
                method: 'PUT',
                headers: {
                    'Authorization': `Bearer ${authToken}`,
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(payload)
            })
            .then(res => res.json())
            .then(result => {
                if (result.success) {
                    alert('Status updated!');
                    bootstrap.Modal.getInstance(document.getElementById('orderStatusModal')).hide();
                    loadSectionContent('orders');
                } else {
                    alert(result.message || 'Failed');
                }
            })
            .catch(err => alert('Error: ' + err));
        });


        function deleteOrder(id) {
            if (!confirm('Delete this order permanently?')) return;

            fetch(`${API_BASE}/admin/orders/${id}`, {
                method: 'DELETE',
                headers: { 'Authorization': `Bearer ${authToken}` }
            })
            .then(res => res.json())
            .then(result => {
                if (result.success) {
                    alert('Order deleted');
                    loadSectionContent('orders');
                } else {
                    alert(result.message);
                }
            });
        }

        // Action Functions - Users
        let currentUserId = null;

        function addNewUser() {
            currentUserId = null;
            document.getElementById('userModalTitle').textContent = 'Add New User';
            document.getElementById('userForm').reset();
            document.getElementById('userId').value = '';
            document.getElementById('userCurrentImage').style.display = 'none';
            document.getElementById('passwordLabel').innerHTML = 'Password <span class="text-danger">*</span>';
            document.getElementById('passwordHint').style.display = 'none';
            document.getElementById('userPassword').required = true;
            loadRolesForUser();
            new bootstrap.Modal(document.getElementById('userModal')).show();
        }

        function editUser(id) {
            currentUserId = id;
            document.getElementById('userModalTitle').textContent = 'Edit User';
            document.getElementById('passwordLabel').innerHTML = 'New Password';
            document.getElementById('passwordHint').style.display = 'block';
            document.getElementById('userPassword').required = false;

            Promise.all([
                fetch(`${API_BASE}/admin/users/${id}`, { headers: { 'Authorization': `Bearer ${authToken}` } }).then(r => r.json()),
                loadRolesForUser()
            ])
            .then(([userRes]) => {
                if (userRes.success) {
                    const u = userRes.data;
                    document.getElementById('userId').value = u.userId;
                    document.getElementById('userName').value = u.userName;
                    document.getElementById('userEmail').value = u.email;
                    document.getElementById('userPhone').value = u.phoneNumber || '';
                    document.getElementById('userAddress').value = u.address || '';
                    document.getElementById('userRole').value = u.roles[0] || '';

                    const img = document.getElementById('userCurrentImage');
                    if (u.imageUrl && !u.imageUrl.includes('default')) {
                        img.src = u.imageUrl;
                        img.style.display = 'block';
                    } else {
                        img.style.display = 'none';
                    }

                    new bootstrap.Modal(document.getElementById('userModal')).show();
                } else {
                    alert(userRes.message);
                }
            })
            .catch(err => alert('Failed to load user: ' + err));
        }

        function deleteUser(id) {
            if (!confirm('Are you sure you want to delete this user? This action is permanent.')) return;

            fetch(`${API_BASE}/admin/users/${id}`, {
                method: 'DELETE',
                headers: { 'Authorization': `Bearer ${authToken}` }
            })
            .then(res => res.json())
            .then(result => {
                if (result.success) {
                    alert('User deleted successfully');
                    loadSectionContent('users');
                } else {
                    alert(result.message || 'Failed to delete');
                }
            })
            .catch(err => alert('Error: ' + err));
        }

        // Load roles from existing users
        async function loadRolesForUser() {
            try {
                const res = await fetch(`${API_BASE}/admin/users`, {
                    headers: { 'Authorization': `Bearer ${authToken}` }
                });
                const result = await res.json();
                if (result.success && result.data.length > 0) {
                    const roles = [...new Set(result.data.flatMap(u => u.roles))];
                    const select = document.getElementById('userRole');
                    select.innerHTML = '<option value="">Select Role</option>' + 
                        roles.map(r => `<option value="${r}">${r}</option>`).join('');
                }
            } catch (err) {
                console.error('Failed to load roles', err);
            }
        }

        // Handle form submit
        document.getElementById('userForm').addEventListener('submit', function(e) {
            e.preventDefault();

            const formData = new FormData();
            formData.append('UserName', document.getElementById('userName').value.trim());
            formData.append('Email', document.getElementById('userEmail').value.trim());
            formData.append('PhoneNumber', document.getElementById('userPhone').value);
            formData.append('Address', document.getElementById('userAddress').value);
            formData.append('Role', document.getElementById('userRole').value);

            const password = document.getElementById('userPassword').value;
            if (currentUserId === null || password) {
                formData.append('Password', password);
            }

            const imageFile = document.getElementById('userImage').files[0];
            if (imageFile) formData.append('ImageFile', imageFile);

            const id = document.getElementById('userId').value;
            const method = id ? 'PUT' : 'POST';
            const url = id ? `${API_BASE}/admin/users/${id}` : `${API_BASE}/admin/users`;

            fetch(url, {
                method: method,
                headers: { 'Authorization': `Bearer ${authToken}` },
                body: formData
            })
            .then(res => res.json())
            .then(result => {
                if (result.success) {
                    alert(id ? 'User updated successfully!' : 'User created successfully!');
                    bootstrap.Modal.getInstance(document.getElementById('userModal')).hide();
                    loadSectionContent('users');
                } else {
                    alert(result.message || 'Operation failed');
                }
            })
            .catch(err => alert('Error: ' + err));
        });

        // Action Functions - Customers

        function viewCustomer(id) {
            Promise.all([
                fetch(`${API_BASE}/admin/users/${id}`, { headers: { 'Authorization': `Bearer ${authToken}` } }).then(r => r.json()),
                fetch(`${API_BASE}/admin/orders`, { headers: { 'Authorization': `Bearer ${authToken}` } }).then(r => r.json())
            ])
            .then(([userRes, ordersRes]) => {
                if (!userRes.success) {
                    alert(userRes.message); return;
                }

                const customer = userRes.data;
                const allOrders = ordersRes.success ? ordersRes.data : [];
                const customerOrders = allOrders.filter(o => o.userId === id);

                const totalSpent = customerOrders.reduce((sum, o) => sum + o.total, 0);
                const lastOrder = customerOrders[0] ? new Date(customerOrders[0].createdAt).toLocaleDateString() : 'N/A';

                const ordersHtml = customerOrders.length > 0 ? customerOrders.map(order => `
                    <tr>
                        <td>#${order.id}</td>
                        <td>${new Date(order.createdAt).toLocaleString()}</td>
                        <td>${order.orderType}</td>
                        <td>$${order.total.toFixed(2)}</td>
                        <td>
                            <span class="badge ${getStatusBadgeClass(order.status)}">${order.status}</span>
                        </td>
                        <td class="text-center">
                            <button class="btn btn-sm btn-primary me-1" onclick="updateOrderStatusFromCustomer(${order.id})">
                                <i class="fas fa-edit"></i>
                            </button>
                            <button class="btn btn-sm btn-danger" onclick="deleteOrderFromCustomer(${order.id})">
                                <i class="fas fa-trash"></i>
                            </button>
                        </td>
                    </tr>
                `).join('') : '<tr><td colspan="6" class="text-center text-muted">No orders yet</td></tr>';

                document.getElementById('customerDetailsBody').innerHTML = `
                    <div class="row mb-4">
                        <div class="col-md-4 text-center">
                            <img src="${customer.imageUrl || '/images/users/default-avatar.jpg'}" 
                                class="img-fluid rounded-circle mb-3" style="width: 120px; height: 120px; object-fit: cover;">
                            <h5>${customer.userName}</h5>
                            <p class="text-muted">${customer.email}</p>
                        </div>
                        <div class="col-md-8">
                            <div class="row g-3">
                                <div class="col-6"><strong>Phone:</strong></div>
                                <div class="col-6">${customer.phoneNumber || 'N/A'}</div>
                                <div class="col-6"><strong>Address:</strong></div>
                                <div class="col-6"><small>${customer.address || 'N/A'}</small></div>
                                <div class="col-6"><strong>Total Orders:</strong></div>
                                <div class="col-6"><span class="badge bg-info">${customerOrders.length}</span></div>
                                <div class="col-6"><strong>Total Spent:</strong></div>
                                <div class="col-6"><strong class="text-success">$${totalSpent.toFixed(2)}</strong></div>
                                <div class="col-6"><strong>Last Order:</strong></div>
                                <div class="col-6">${lastOrder}</div>
                                <div class="col-6"><strong>Member Since:</strong></div>
                                <div class="col-6">${new Date(customer.createdAt).toLocaleDateString()}</div>
                            </div>
                        </div>
                    </div>
                    <hr>
                    <h6>Customer Orders</h6>
                    <div class="table-responsive">
                        <table class="table table-sm table-hover">
                            <thead>
                                <tr>
                                    <th>Order ID</th>
                                    <th>Date</th>
                                    <th>Type</th>
                                    <th>Total</th>
                                    <th>Status</th>
                                    <th>Actions</th>
                                </tr>
                            </thead>
                            <tbody>${ordersHtml}</tbody>
                        </table>
                    </div>
                `;

                new bootstrap.Modal(document.getElementById('customerDetailsModal')).show();
            })
            .catch(err => {
                console.error(err);
                alert('Failed to load customer details');
            });
        } 
        
        
      
       function updateOrderStatusFromCustomer(orderId) {
            currentOrderId = orderId;
            document.getElementById('orderIdForStatus').value = orderId;
            document.getElementById('orderStatusForm').reset();

            fetch(`${API_BASE}/admin/orders/${orderId}`, {
                headers: { 'Authorization': `Bearer ${authToken}` }
            })
            .then(res => res.json())
            .then(result => {
                if (result.success) {
                    document.getElementById('newOrderStatus').value = result.data.status;
                    new bootstrap.Modal(document.getElementById('orderStatusModal')).show();
                }
            });
        }
        function deleteOrderFromCustomer(orderId) {
            if (!confirm('Delete this order permanently?')) return;

            fetch(`${API_BASE}/admin/orders/${orderId}`, {
                method: 'DELETE',
                headers: { 'Authorization': `Bearer ${authToken}` }
            })
            .then(res => res.json())
            .then(result => {
                if (result.success) {
                    alert('Order deleted');
                    const customerId = document.querySelector('#customerDetailsModal').dataset.customerId;
                    if (customerId) viewCustomer(customerId);
                    loadSectionContent('orders');
                } else {
                    alert(result.message);
                }
            });
        }
        
        

        function editCustomer(id) {
            editUser(id);
        }

        function deleteCustomer(id) {
            if (!confirm('Are you sure you want to delete this customer?')) return;

            fetch(`${API_BASE}/admin/users/${id}`, {
                method: 'DELETE',
                headers: { 'Authorization': `Bearer ${authToken}` }
            })
            .then(res => res.json())
            .then(result => {
                if (result.success) {
                    alert('Customer deleted successfully');
                    loadSectionContent('customers');
                } else {
                    alert(result.message || 'Failed');
                }
            })
            .catch(err => alert('Error: ' + err));
        }

        // Action Functions - Wishlists

        function viewWishlist(id) {
            fetch(`${API_BASE}/admin/wishlists/${id}`, {
                headers: { 'Authorization': `Bearer ${authToken}` }
            })
            .then(res => res.json())
            .then(result => {
                if (result.success) {
                    const w = result.data;
                    const itemsHtml = w.wishlistItems.length > 0 ? w.wishlistItems.map(item => `
                        <tr>
                            <td>
                                <img src="${item.menuItemImageUrl || 'https://via.placeholder.com/50'}" 
                                    alt="${item.menuItemName}" 
                                    style="width: 50px; height: 50px; object-fit: cover; border-radius: 8px; margin-right: 10px;">
                                ${item.menuItemName}
                            </td>
                            <td>${item.desiredQuantity}</td>
                            <td>$${item.price.toFixed(2)}</td>
                            <td>$${item.subtotal.toFixed(2)}</td>
                            <td>
                                <button class="btn btn-sm btn-danger" onclick="deleteWishlistItem(${item.menuItemId}, ${w.id})">
                                    <i class="fas fa-trash"></i>
                                </button>
                            </td>
                        </tr>
                    `).join('') : '<tr><td colspan="5" class="text-center text-muted">No items in wishlist</td></tr>';

                    document.getElementById('wishlistDetailsBody').innerHTML = `
                        <div class="row mb-3">
                            <div class="col-md-6">
                                <p><strong>Wishlist ID:</strong> #${w.id}</p>
                                <p><strong>Customer:</strong> ${w.userName}</p>
                                <p><strong>Email:</strong> ${w.userEmail}</p>
                            </div>
                            <div class="col-md-6 text-end">
                                <p><strong>Items Count:</strong> <span class="badge bg-info">${w.itemsCount}</span></p>
                                <p><strong>Total Value:</strong> <strong class="text-success">$${w.totalEstimatedPrice.toFixed(2)}</strong></p>
                                <p><strong>Created:</strong> ${new Date(w.createdAt).toLocaleString()}</p>
                            </div>
                        </div>
                        <hr>
                        <h6>Wishlist Items</h6>
                        <div class="table-responsive">
                            <table class="table table-sm table-hover">
                                <thead>
                                    <tr>
                                        <th>Item</th>
                                        <th>Qty</th>
                                        <th>Price</th>
                                        <th>Subtotal</th>
                                        <th>Action</th>
                                    </tr>
                                </thead>
                                <tbody>${itemsHtml}</tbody>
                            </table>
                        </div>
                    `;
                    new bootstrap.Modal(document.getElementById('wishlistDetailsModal')).show();
                } else {
                    alert(result.message || 'Failed to load wishlist');
                }
            })
            .catch(err => alert('Error: ' + err));
        }

        function deleteWishlist(id) {
            if (!confirm('Are you sure you want to delete this entire wishlist?')) return;

            fetch(`${API_BASE}/admin/wishlists/${id}`, {
                method: 'DELETE',
                headers: { 'Authorization': `Bearer ${authToken}` }
            })
            .then(res => res.json())
            .then(result => {
                if (result.success) {
                    alert('Wishlist deleted successfully');
                    loadSectionContent('wishlists');
                } else {
                    alert(result.message || 'Failed to delete');
                }
            })
            .catch(err => alert('Error: ' + err));
        }

        // Delete single item from wishlist
        function deleteWishlistItem(menuItemId, wishlistId) {
            if (!confirm('Remove this item from the wishlist?')) return;

            // First, get the wishlist item ID (we need to find it)
            fetch(`${API_BASE}/admin/wishlists/${wishlistId}`, {
                headers: { 'Authorization': `Bearer ${authToken}` }
            })
            .then(res => res.json())
            .then(result => {
                if (result.success) {
                    const item = result.data.wishlistItems.find(i => i.menuItemId === menuItemId);
                    if (item) {
                        // Now delete the item
                        fetch(`${API_BASE}/admin/wishlists/items/${item.id}`, {
                            method: 'DELETE',
                            headers: { 'Authorization': `Bearer ${authToken}` }
                        })
                        .then(res => res.json())
                        .then(delRes => {
                            if (delRes.success) {
                                alert('Item removed from wishlist');
                                viewWishlist(wishlistId); // Refresh modal
                                loadSectionContent('wishlists'); // Refresh table
                            } else {
                                alert(delRes.message);
                            }
                        });
                    }
                }
            })
            .catch(err => alert('Error: ' + err));
        }
        
        // Logout Function
        function logout() {
            localStorage.removeItem('jwt_token');
            localStorage.removeItem('user_roles');
            window.location.href = 'Account.html';
        }
    