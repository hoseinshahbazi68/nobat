// Swagger Login Custom Script
(function() {
    'use strict';

    // Wait for Swagger UI to be ready
    window.addEventListener('load', function() {
        // Wait a bit more for Swagger UI to fully initialize
        setTimeout(function() {
            addLoginButton();
        }, 1000);
    });

    function addLoginButton() {
        // Find the authorize button container
        const authorizeBtn = document.querySelector('.btn.authorize');
        if (!authorizeBtn) {
            // Try again after a delay
            setTimeout(addLoginButton, 500);
            return;
        }

        // Create login button
        const loginBtn = document.createElement('button');
        loginBtn.className = 'btn login-btn';
        loginBtn.innerHTML = '<span>🔐 ورود</span>';
        loginBtn.style.cssText = 'margin-right: 10px; background-color: #4CAF50; color: white; border: none; padding: 8px 16px; border-radius: 4px; cursor: pointer;';

        // Add click handler
        loginBtn.addEventListener('click', function() {
            showLoginModal();
        });

        // Insert before authorize button
        authorizeBtn.parentNode.insertBefore(loginBtn, authorizeBtn);
    }

    function showLoginModal() {
        // Remove existing modal if any
        const existingModal = document.getElementById('swagger-login-modal');
        if (existingModal) {
            existingModal.remove();
        }

        // Create modal
        const modal = document.createElement('div');
        modal.id = 'swagger-login-modal';
        modal.style.cssText = 'position: fixed; top: 0; left: 0; width: 100%; height: 100%; background-color: rgba(0,0,0,0.5); z-index: 10000; display: flex; justify-content: center; align-items: center;';

        // Create modal content
        const modalContent = document.createElement('div');
        modalContent.style.cssText = 'background: white; padding: 30px; border-radius: 8px; max-width: 400px; width: 90%; box-shadow: 0 4px 6px rgba(0,0,0,0.1);';

        modalContent.innerHTML = `
            <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px;">
                <h2 style="margin: 0; color: #333;">ورود به سیستم</h2>
                <button id="close-login-modal" style="background: none; border: none; font-size: 24px; cursor: pointer; color: #666;">&times;</button>
            </div>
            <form id="login-form" style="display: flex; flex-direction: column; gap: 15px;">
                <div>
                    <label style="display: block; margin-bottom: 5px; color: #333; font-weight: 500;">نام کاربری:</label>
                    <input type="text" id="login-username" required
                           style="width: 100%; padding: 10px; border: 1px solid #ddd; border-radius: 4px; box-sizing: border-box; font-size: 14px;">
                </div>
                <div>
                    <label style="display: block; margin-bottom: 5px; color: #333; font-weight: 500;">رمز عبور:</label>
                    <input type="password" id="login-password" required
                           style="width: 100%; padding: 10px; border: 1px solid #ddd; border-radius: 4px; box-sizing: border-box; font-size: 14px;">
                </div>
                <div id="login-error" style="color: red; display: none; font-size: 14px;"></div>
                <div style="display: flex; gap: 10px; justify-content: flex-end;">
                    <button type="button" id="cancel-login"
                            style="padding: 10px 20px; border: 1px solid #ddd; background: white; border-radius: 4px; cursor: pointer; color: #333;">
                        انصراف
                    </button>
                    <button type="submit"
                            style="padding: 10px 20px; border: none; background: #4CAF50; color: white; border-radius: 4px; cursor: pointer; font-weight: 500;">
                        ورود
                    </button>
                </div>
            </form>
        `;

        modal.appendChild(modalContent);
        document.body.appendChild(modal);

        // Close modal handlers
        document.getElementById('close-login-modal').addEventListener('click', function() {
            modal.remove();
        });

        document.getElementById('cancel-login').addEventListener('click', function() {
            modal.remove();
        });

        modal.addEventListener('click', function(e) {
            if (e.target === modal) {
                modal.remove();
            }
        });

        // Form submit handler
        document.getElementById('login-form').addEventListener('submit', function(e) {
            e.preventDefault();
            performLogin();
        });

        // Focus on username field
        document.getElementById('login-username').focus();
    }

    function performLogin() {
        const username = document.getElementById('login-username').value;
        const password = document.getElementById('login-password').value;
        const errorDiv = document.getElementById('login-error');
        const submitBtn = document.querySelector('#login-form button[type="submit"]');

        // Clear previous error
        errorDiv.style.display = 'none';
        errorDiv.textContent = '';
        submitBtn.disabled = true;
        submitBtn.textContent = 'در حال ورود...';

        // Get base URL
        const baseUrl = window.location.origin;

        // Perform login
        fetch(baseUrl + '/api/v1/auth/login', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                username: username,
                password: password
            })
        })
        .then(response => {
            if (!response.ok) {
                return response.text().then(text => {
                    throw new Error(text || 'خطا در ورود');
                });
            }
            return response.json();
        })
        .then(data => {
            if (data.token) {
                // Set token in Swagger UI
                setSwaggerToken(data.token);

                // Close modal
                document.getElementById('swagger-login-modal').remove();

                // Show success message
                showMessage('ورود با موفقیت انجام شد!', 'success');
            } else {
                throw new Error('توکن دریافت نشد');
            }
        })
        .catch(error => {
            errorDiv.textContent = error.message || 'خطا در ورود. لطفاً دوباره تلاش کنید.';
            errorDiv.style.display = 'block';
            submitBtn.disabled = false;
            submitBtn.textContent = 'ورود';
        });
    }

    function setSwaggerToken(token) {
        // Try to use Swagger UI API first
        if (window.ui && window.ui.authActions) {
            try {
                window.ui.authActions.authorize({
                    Bearer: {
                        name: 'Bearer',
                        schema: {
                            type: 'apiKey',
                            in: 'header',
                            name: 'Authorization',
                            description: 'JWT Authorization header using the Bearer scheme'
                        },
                        value: token
                    }
                });
                return;
            } catch (e) {
                console.log('Swagger UI API not available, trying DOM method');
            }
        }

        // Fallback: Use DOM manipulation
        const authorizeBtn = document.querySelector('.btn.authorize');
        if (authorizeBtn) {
            // Click to open authorize modal
            authorizeBtn.click();

            // Wait for modal to open
            setTimeout(function() {
                // Find the input field for Bearer token
                let tokenInput = null;

                // Try different selectors
                const selectors = [
                    'input[placeholder*="Bearer"]',
                    'input[placeholder*="token"]',
                    'input[type="text"]',
                    'input[name*="token"]',
                    'input[name*="Bearer"]'
                ];

                for (let selector of selectors) {
                    const inputs = document.querySelectorAll(selector);
                    for (let input of inputs) {
                        const modal = input.closest('.dialog-ux, .modal-ux, [class*="modal"]');
                        if (modal && modal.style.display !== 'none') {
                            tokenInput = input;
                            break;
                        }
                    }
                    if (tokenInput) break;
                }

                if (tokenInput) {
                    tokenInput.value = token;
                    tokenInput.dispatchEvent(new Event('input', { bubbles: true }));
                    tokenInput.dispatchEvent(new Event('change', { bubbles: true }));
                }

                // Find and click authorize button in modal
                setTimeout(function() {
                    const modal = document.querySelector('.dialog-ux, .modal-ux, [class*="modal"]');
                    if (modal) {
                        const buttons = modal.querySelectorAll('button');
                        for (let btn of buttons) {
                            const text = btn.textContent.trim().toLowerCase();
                            if (text.includes('authorize') || text.includes('تأیید') || text.includes('ذخیره')) {
                                btn.click();
                                break;
                            }
                        }
                    }
                }, 200);
            }, 300);
        }
    }

    function showMessage(message, type) {
        // Create a temporary message element
        const messageDiv = document.createElement('div');
        messageDiv.style.cssText = `
            position: fixed;
            top: 20px;
            right: 20px;
            background: ${type === 'success' ? '#4CAF50' : '#f44336'};
            color: white;
            padding: 15px 20px;
            border-radius: 4px;
            box-shadow: 0 2px 5px rgba(0,0,0,0.2);
            z-index: 10001;
            font-size: 14px;
        `;
        messageDiv.textContent = message;
        document.body.appendChild(messageDiv);

        // Remove after 3 seconds
        setTimeout(function() {
            messageDiv.remove();
        }, 3000);
    }
})();
