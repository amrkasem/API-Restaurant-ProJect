
    //  API Base 
    let API_BASE = 'https://localhost:7201/api';

    document.getElementById('apiEndpoint').addEventListener('change', function() {
        API_BASE = this.value;
        console.log('API Base updated to:', API_BASE);
    });

    function showTab(tabName) {
        document.querySelectorAll('.tab').forEach(tab => tab.classList.remove('active'));
        document.querySelectorAll('.tab-content').forEach(content => content.classList.remove('active'));
        
        event.target.classList.add('active');
        document.getElementById(tabName).classList.add('active');
    }

    // Register Form Handler
    document.getElementById('registerForm').addEventListener('submit', async (e) => {
        e.preventDefault();
        const formData = new FormData(e.target);
        
        showResponse('registerResponse', 'Loading...', 'success');

        try {
            const response = await fetch(`${API_BASE}/Account/register`, {
                method: 'POST',
                body: formData
            });

            const data = await response.json();

            if (response.ok) {
                showResponse('registerResponse', ` ${data.message}`, 'success', data);
                
                //  Redirect to Customer Dashboard after successful registration
                setTimeout(() => {
                    window.location.href = 'CustomerDashboard.html';
                }, 2000);
                
                e.target.reset();
            } else {
                showResponse('registerResponse', ` ${data.message}`, 'error', data);
            }
        } catch (error) {
            showResponse('registerResponse', ` Error: ${error.message}`, 'error');
        }
    });

// Login Form Handler
document.getElementById('loginForm').addEventListener('submit', async (e) => {
    e.preventDefault();
    const formData = new FormData(e.target);
    const data = Object.fromEntries(formData);

    showResponse('loginResponse', 'Loading...', 'success');

    try {
        const response = await fetch(`${API_BASE}/Account/login`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(data)
        });

        if (!response.ok) {
            const errorText = await response.text();
            showResponse('loginResponse', `Login failed: ${response.status}`, 'error');
            return;
        }

        const result = await response.json();

        if (result.success) {
            showResponse('loginResponse', `Login successful`, 'success', result);

           
            if (result.data && result.data.token) {
                localStorage.setItem('jwt_token', result.data.token);
                localStorage.setItem('user_roles', JSON.stringify(result.data.roles || []));
                console.log('Token & Roles saved to localStorage');
            }
           

            const tokenDisplay = document.createElement('div');
            tokenDisplay.className = 'token-display';
            tokenDisplay.innerHTML = `<strong>JWT Token:</ добавить<br>${result.data.token.substring(0, 50)}...`;
            document.getElementById('loginResponse').appendChild(tokenDisplay);

            addRedirectButton(result.data.roles || [], result.data.email, result.data.userName);
        } else {
            showResponse('loginResponse', `Failed: ${result.message}`, 'error', result);
        }
    } catch (error) {
        showResponse('loginResponse', `Error: ${error.message}`, 'error');
    }
});  
    //  Function to add redirect button based on user role or email
    function addRedirectButton(roles, email, userName) {
        const loginResponse = document.getElementById('loginResponse');
        
        // Remove existing redirect button if any
        const existingBtn = document.getElementById('redirectBtn');
        if (existingBtn) {
            existingBtn.remove();
        }

        const redirectBtn = document.createElement('button');
        redirectBtn.id = 'redirectBtn';
        redirectBtn.className = 'redirect-btn';
        
        let dashboardType = 'Customer';
        let detectedBy = 'default';
        
        if (roles && roles.length > 0) {
            if (roles.includes('Admin')) {
                dashboardType = 'Admin';
                detectedBy = 'role';
            } else if (roles.includes('Customer')) {
                dashboardType = 'Customer';
                detectedBy = 'role';
            }
        } else {
            if (email === 'admin@restaurant.com' || userName === 'admin@restaurant.com') {
                dashboardType = 'Admin';
                detectedBy = 'email';
            } else if (email.includes('admin') || userName.includes('admin')) {
                dashboardType = 'Admin';
                detectedBy = 'email pattern';
            } else {
                dashboardType = 'Customer';
                detectedBy = 'email default';
            }
        }
        
        if (dashboardType === 'Admin') {
            redirectBtn.textContent = ' Go to Admin Dashboard';
            redirectBtn.onclick = () => {
                window.location.href = 'AdminDashboard.html';
            };
        } else {
            redirectBtn.textContent = ' Go to Customer Dashboard';
            redirectBtn.onclick = () => {
                window.location.href = 'CustomerDashboard.html';
            };
        }
        
        loginResponse.appendChild(redirectBtn);
        
        const roleInfo = document.createElement('div');
        roleInfo.style.marginTop = '10px';
        roleInfo.style.padding = '10px';
        roleInfo.style.background = '#e9ecef';
        roleInfo.style.borderRadius = '5px';
        roleInfo.style.fontSize = '12px';
        roleInfo.innerHTML = `
            <strong>User Info:</strong><br>
            - Email: ${email}<br>
            - Username: ${userName}<br>
            - Roles: ${roles && roles.length > 0 ? roles.join(', ') : 'None'}<br>
            - Detected as: <strong>${dashboardType}</strong> (by ${detectedBy})
        `;
        loginResponse.appendChild(roleInfo);
    }

    // Get Profile
    async function getProfile() {
        const token = localStorage.getItem('jwt_token');
        
        if (!token) {
            showResponse('profileResponse', ' Please login first to get your profile', 'error');
            return;
        }

        showResponse('profileResponse', 'Loading...', 'success');

        try {
            const response = await fetch(`${API_BASE}/Account/profile`, {
                method: 'GET',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                }
            });

            const data = await response.json();

            if (response.ok) {
                showResponse('profileResponse', ` Profile retrieved successfully`, 'success', data);
            } else {
                showResponse('profileResponse', ` ${data.message}`, 'error', data);
            }
        } catch (error) {
            showResponse('profileResponse', ` Error: ${error.message}`, 'error');
        }
    }

    // Update Profile
    async function updateProfile() {
        const token = localStorage.getItem('jwt_token');
        
        if (!token) {
            showResponse('profileResponse', ' Please login first to update your profile', 'error');
            return;
        }

        const phoneNumber = prompt('Enter new phone number:');
        const address = prompt('Enter new address:');

        if (!phoneNumber && !address) {
            showResponse('profileResponse', ' Please provide at least one field to update', 'error');
            return;
        }

        showResponse('profileResponse', 'Loading...', 'success');

        try {
            const response = await fetch(`${API_BASE}/Account/profile`, {
                method: 'PUT',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    phoneNumber: phoneNumber || '',
                    address: address || ''
                })
            });

            const data = await response.json();

            if (response.ok) {
                showResponse('profileResponse', ` ${data.message}`, 'success', data);
            } else {
                showResponse('profileResponse', ` ${data.message}`, 'error', data);
            }
        } catch (error) {
            showResponse('profileResponse', ` Error: ${error.message}`, 'error');
        }
    }

    // Initialize System
    async function initializeSystem() {
        showResponse('initializeResponse', 'Loading...', 'success');

        try {
            const response = await fetch(`${API_BASE}/Account/initialize-system`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            const data = await response.json();

            if (response.ok) {
                showResponse('initializeResponse', ` ${data.message}`, 'success', data);
                
                //  Redirect to Admin Dashboard after system initialization
                setTimeout(() => {
                    window.location.href = 'AdminDashboard.html';
                }, 2000);
            } else {
                showResponse('initializeResponse', ` ${data.message}`, 'error', data);
            }
        } catch (error) {
            showResponse('initializeResponse', ` Error: ${error.message}`, 'error');
        }
    }

    // Show Response Helper
    function showResponse(elementId, message, type, data = null) {
        const responseDiv = document.getElementById(elementId);
        responseDiv.className = `response ${type}`;
        responseDiv.style.display = 'block';
        
        let html = `<strong>${message}</strong>`;
        
        if (data) {
            html += `<pre>${JSON.stringify(data, null, 2)}</pre>`;
        }
        
        responseDiv.innerHTML = html;
    }

    // Check if user is logged in on page load
    window.addEventListener('load', () => {
        const token = localStorage.getItem('jwt_token');
        const roles = localStorage.getItem('user_roles');
        
        if (token && roles) {
            console.log('User is logged in with roles:', JSON.parse(roles));
        }
    });
