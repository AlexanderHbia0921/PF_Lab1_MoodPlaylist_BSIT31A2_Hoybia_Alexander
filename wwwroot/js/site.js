// Modern Music App UI Enhancements
document.addEventListener('DOMContentLoaded', function() {
    console.log('Site JS loaded');
    
    // Initialize Music Player
    initializeMusicPlayer();
    
    // Initialize Sidebar
    initializeSidebar();
    
    // Wait for Bootstrap to be available and initialize dropdowns
    setTimeout(() => {
        if (typeof bootstrap !== 'undefined') {
            console.log('Bootstrap loaded successfully');
            
            // Initialize all dropdowns manually
            const dropdowns = document.querySelectorAll('[data-bs-toggle="dropdown"]');
            dropdowns.forEach(dropdown => {
                try {
                    const dropdownInstance = new bootstrap.Dropdown(dropdown);
                    console.log('Dropdown initialized:', dropdown.id);
                } catch (error) {
                    console.error('Error initializing dropdown:', error);
                }
            });
            console.log('Initialized', dropdowns.length, 'dropdowns');
        } else {
            console.error('Bootstrap not loaded');
            
            // Fallback: Add click handlers manually if Bootstrap is not available
            const userDropdown = document.getElementById('userDropdown');
            const dropdownMenu = document.querySelector('.dropdown-menu');
            
            if (userDropdown && dropdownMenu) {
                userDropdown.addEventListener('click', function(e) {
                    e.preventDefault();
                    e.stopPropagation();
                    dropdownMenu.classList.toggle('show');
                });
                
                // Close dropdown when clicking outside
                document.addEventListener('click', function(event) {
                    if (!userDropdown.contains(event.target) && !dropdownMenu.contains(event.target)) {
                        dropdownMenu.classList.remove('show');
                    }
                });
            }
        }
    }, 100);
    
    // Add specific logout button functionality
    const logoutBtn = document.querySelector('form[action*="Logout"] button[type="submit"]');
    if (logoutBtn) {
        console.log('Found logout button:', logoutBtn);
        logoutBtn.addEventListener('click', function(e) {
            console.log('Logout button clicked!');
            // Form will submit naturally, no need to prevent default
        });
    } else {
        console.log('Logout button not found');
    }
    
    // FIXED: Enhanced Theme Toggle functionality
    const themeToggle = document.getElementById('themeToggle');
    const themeIcon = document.getElementById('themeIcon');
    const html = document.documentElement;
    
    console.log('Theme toggle button found:', !!themeToggle);
    console.log('Theme icon found:', !!themeIcon);
    
    // Get current theme (should already be set by inline script, but double-check)
    const currentTheme = html.getAttribute('data-bs-theme') || localStorage.getItem('theme') || 'dark';
    html.setAttribute('data-bs-theme', currentTheme);
    localStorage.setItem('theme', currentTheme);
    updateThemeIcon(currentTheme);
    
    console.log('Current theme:', currentTheme);
    
    // Update sidebar for desktop
    updateSidebarLayout();
    
    if (themeToggle) {
        // Remove any existing event listeners
        themeToggle.onclick = null;
        
        themeToggle.addEventListener('click', function(e) {
            e.preventDefault();
            e.stopPropagation();
            
            console.log('Theme toggle clicked!');
            
            const currentTheme = html.getAttribute('data-bs-theme');
            const newTheme = currentTheme === 'dark' ? 'light' : 'dark';
            
            console.log('Switching from', currentTheme, 'to', newTheme);
            
            // Simple immediate theme switch (no overlay for now to avoid issues)
            html.setAttribute('data-bs-theme', newTheme);
            localStorage.setItem('theme', newTheme);
            updateThemeIcon(newTheme);
            
            console.log('Theme switched successfully');
            
            // Show simple notification
            const notification = document.createElement('div');
            notification.innerHTML = `Switched to ${newTheme} mode`;
            notification.style.cssText = `
                position: fixed;
                top: 20px;
                right: 20px;
                background: var(--spotify-green);
                color: white;
                padding: 12px 20px;
                border-radius: 8px;
                font-size: 14px;
                font-weight: 500;
                z-index: 10000;
                opacity: 0;
                transition: all 0.3s ease;
                box-shadow: 0 4px 12px rgba(0, 0, 0, 0.3);
            `;
            
            document.body.appendChild(notification);
            
            // Show notification
            setTimeout(() => {
                notification.style.opacity = '1';
                notification.style.transform = 'translateY(0)';
            }, 100);
            
            // Hide notification
            setTimeout(() => {
                notification.style.opacity = '0';
                notification.style.transform = 'translateY(-10px)';
                setTimeout(() => {
                    if (notification.parentNode) {
                        notification.parentNode.removeChild(notification);
                    }
                }, 300);
            }, 2000);
        });
        
        console.log('Theme toggle event listener added');
    } else {
        console.error('Theme toggle button not found!');
    }
    
    function updateThemeIcon(theme) {
        console.log('Updating theme icon for:', theme);
        if (themeIcon) {
            if (theme === 'dark') {
                themeIcon.className = 'fas fa-sun';
                if (themeToggle) themeToggle.title = 'Switch to Light Mode';
            } else {
                themeIcon.className = 'fas fa-moon';
                if (themeToggle) themeToggle.title = 'Switch to Dark Mode';
            }
            console.log('Theme icon updated to:', themeIcon.className);
        } else {
            console.error('Theme icon not found!');
        }
    }
    // Navbar scroll effect
    const navbar = document.getElementById('mainNavbar');
    if (navbar) {
        window.addEventListener('scroll', function() {
            if (window.scrollY > 50) {
                navbar.classList.add('scrolled');
            } else {
                navbar.classList.remove('scrolled');
            }
        });
    }

    // Quick Sidebar Toggle
    const sidebarToggle = document.getElementById('sidebarToggle');
    const quickSidebar = document.getElementById('quickSidebar');
    
    if (sidebarToggle && quickSidebar) {
        sidebarToggle.addEventListener('click', function() {
            quickSidebar.classList.toggle('active');
            
            // Update toggle icon
            const icon = this.querySelector('i');
            if (quickSidebar.classList.contains('active')) {
                icon.classList.remove('fa-bars');
                icon.classList.add('fa-times');
            } else {
                icon.classList.remove('fa-times');
                icon.classList.add('fa-bars');
            }
        });
        
        // Close sidebar when clicking outside
        document.addEventListener('click', function(event) {
            if (!quickSidebar.contains(event.target)) {
                quickSidebar.classList.remove('active');
                const icon = sidebarToggle.querySelector('i');
                icon.classList.remove('fa-times');
                icon.classList.add('fa-bars');
            }
        });
    }

    // Counter animation
    const counters = document.querySelectorAll('.counter');
    const animateCounter = (counter) => {
        const target = parseInt(counter.getAttribute('data-target'));
        const increment = target / 100;
        let current = 0;
        
        const updateCounter = () => {
            if (current < target) {
                current += increment;
                counter.textContent = Math.ceil(current);
                setTimeout(updateCounter, 20);
            } else {
                counter.textContent = target;
            }
        };
        
        updateCounter();
    };

    // Intersection Observer for animations
    const observerOptions = {
        threshold: 0.1,
        rootMargin: '0px 0px -50px 0px'
    };

    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                // Trigger counter animations
                if (entry.target.classList.contains('counter')) {
                    animateCounter(entry.target);
                }
                
                // Add visible class for animations
                entry.target.classList.add('animate-visible');
            }
        });
    }, observerOptions);

    // Observe counters and animated elements
    counters.forEach(counter => observer.observe(counter));
    
    const animatedElements = document.querySelectorAll(
        '.animate-fade-in-up, .animate-fade-in-left, .animate-fade-in-right'
    );
    animatedElements.forEach(element => observer.observe(element));

    // Mood card interactions
    const moodCards = document.querySelectorAll('.mood-card');
    moodCards.forEach(card => {
        card.addEventListener('mouseenter', function() {
            this.style.transform = 'translateY(-8px) scale(1.02)';
        });
        
        card.addEventListener('mouseleave', function() {
            this.style.transform = 'translateY(0) scale(1)';
        });
    });

    // Enhanced mobile navigation
    const navbarToggler = document.querySelector('.navbar-toggler');
    const navbarCollapse = document.querySelector('.navbar-collapse');
    
    if (navbarToggler && navbarCollapse) {
        navbarToggler.addEventListener('click', function() {
            // Add smooth animation class
            navbarCollapse.classList.add('collapsing');
            
            setTimeout(() => {
                navbarCollapse.classList.remove('collapsing');
            }, 350);
        });
        
        // Close mobile menu when clicking nav links
        const navLinks = document.querySelectorAll('.navbar-nav .nav-link');
        navLinks.forEach(link => {
            link.addEventListener('click', () => {
                if (window.innerWidth < 992) {
                    const bsCollapse = new bootstrap.Collapse(navbarCollapse, {
                        toggle: false
                    });
                    bsCollapse.hide();
                }
            });
        });
    }

    // Enhanced loading states for buttons
    const buttons = document.querySelectorAll('.btn');
    buttons.forEach(button => {
        button.addEventListener('click', function(e) {
            // Skip if it's a dropdown toggle or has no href/action
            if (this.hasAttribute('data-bs-toggle') || this.type === 'button') {
                return;
            }
            
            if (this.classList.contains('btn-primary') || this.classList.contains('btn-success')) {
                const originalText = this.innerHTML;
                const originalClasses = this.className;
                
                this.innerHTML = '<span class="loading-spinner me-2"></span>Loading...';
                this.disabled = true;
                this.classList.add('loading');
                
                // Add visual feedback
                this.style.pointerEvents = 'none';
                
                // Restore after navigation (reduced time for better UX)
                setTimeout(() => {
                    this.innerHTML = originalText;
                    this.disabled = false;
                    this.className = originalClasses;
                    this.style.pointerEvents = 'auto';
                }, 800);
            }
        });
        
        // Add hover feedback for all buttons
        button.addEventListener('mouseenter', function() {
            if (!this.disabled) {
                this.style.transform = 'translateY(-1px)';
            }
        });
        
        button.addEventListener('mouseleave', function() {
            if (!this.disabled) {
                this.style.transform = 'translateY(0)';
            }
        });
    });

    // Smooth scroll for anchor links
    const anchorLinks = document.querySelectorAll('a[href^="#"]');
    anchorLinks.forEach(link => {
        link.addEventListener('click', function(e) {
            e.preventDefault();
            const targetId = this.getAttribute('href').substring(1);
            const targetElement = document.getElementById(targetId);
            
            if (targetElement) {
                targetElement.scrollIntoView({
                    behavior: 'smooth',
                    block: 'start'
                });
            }
        });
    });

    // Dynamic theme colors (optional enhancement)
    const setThemeColor = (color) => {
        document.documentElement.style.setProperty('--primary-color', color);
    };

    // Add hover effects to cards
    const cards = document.querySelectorAll('.card');
    cards.forEach(card => {
        card.addEventListener('mouseenter', function() {
            if (!this.classList.contains('no-hover')) {
                this.style.transform = 'translateY(-2px)';
            }
        });
        
        card.addEventListener('mouseleave', function() {
            if (!this.classList.contains('no-hover')) {
                this.style.transform = 'translateY(0)';
            }
        });
    });

    // Toast notifications (utility function)
    window.showToast = function(message, type = 'info') {
        const toastContainer = document.getElementById('toast-container') || createToastContainer();
        const toast = document.createElement('div');
        toast.className = `toast align-items-center text-white bg-${type} border-0`;
        toast.setAttribute('role', 'alert');
        toast.innerHTML = `
            <div class="d-flex">
                <div class="toast-body">${message}</div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
            </div>
        `;
        
        toastContainer.appendChild(toast);
        const bsToast = new bootstrap.Toast(toast);
        bsToast.show();
        
        setTimeout(() => {
            toast.remove();
        }, 5000);
    };

    function createToastContainer() {
        const container = document.createElement('div');
        container.id = 'toast-container';
        container.className = 'toast-container position-fixed top-0 end-0 p-3';
        container.style.zIndex = '1080';
        document.body.appendChild(container);
        return container;
    }
    
    // Page transition effects
    const main = document.querySelector('main');
    if (main) {
        main.classList.add('page-transition');
        setTimeout(() => {
            main.classList.add('loaded');
        }, 100);
    }
    
    // Enhanced accessibility - keyboard navigation
    document.addEventListener('keydown', function(e) {
        // ESC to close sidebar
        if (e.key === 'Escape') {
            const quickSidebar = document.getElementById('quickSidebar');
            const sidebarToggle = document.getElementById('sidebarToggle');
            
            if (quickSidebar && quickSidebar.classList.contains('active')) {
                quickSidebar.classList.remove('active');
                if (sidebarToggle) {
                    const icon = sidebarToggle.querySelector('i');
                    icon.classList.remove('fa-times');
                    icon.classList.add('fa-bars');
                }
            }
        }
        
        // Alt + M for mobile menu toggle
        if (e.altKey && e.key === 'm') {
            e.preventDefault();
            const navbarToggler = document.querySelector('.navbar-toggler');
            if (navbarToggler && window.innerWidth < 992) {
                navbarToggler.click();
            }
        }
    });
    
    // Performance optimization - lazy load animations
    const observerOptions = {
        threshold: 0.1,
        rootMargin: '0px 0px -20px 0px'
    };
    
    const animationObserver = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('animate-visible');
                // Stop observing after animation is triggered
                animationObserver.unobserve(entry.target);
            }
        });
    }, observerOptions);
    
    // Observe all elements with animation classes
    const animatedElements = document.querySelectorAll(
        '[class*="animate-"]'
    );
    animatedElements.forEach(element => {
        // Add initial state
        if (!element.classList.contains('animate-visible')) {
            element.style.opacity = '0';
            element.style.transform = 'translateY(20px)';
        }
        animationObserver.observe(element);
    });
    
    // Network status indicator
    window.addEventListener('online', () => {
        if (window.showToast) {
            window.showToast('You are back online!', 'success');
        }
    });
    
    window.addEventListener('offline', () => {
        if (window.showToast) {
            window.showToast('You are currently offline', 'warning');
        }
    });
});

// Utility functions
function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}

// ===== MUSIC PLAYER FUNCTIONALITY =====

function initializeMusicPlayer() {
    const bottomPlayer = document.getElementById('bottomPlayer');
    if (!bottomPlayer) return;
    
    // Music player state
    let isPlaying = false;
    let currentTrack = null;
    let currentTime = 0;
    let duration = 0;
    let volume = 70;
    
    // Player elements
    const playPauseBtn = document.getElementById('playPauseBtn');
    const playPauseIcon = document.getElementById('playPauseIcon');
    const progressBar = document.getElementById('progressBar');
    const progressFill = document.getElementById('progressFill');
    const currentTimeEl = document.getElementById('currentTime');
    const totalTimeEl = document.getElementById('totalTime');
    const volumeSlider = document.getElementById('volumeSlider');
    const volumeBtn = document.getElementById('volumeBtn');
    const volumeIcon = document.getElementById('volumeIcon');
    const shuffleBtn = document.getElementById('shuffleBtn');
    const repeatBtn = document.getElementById('repeatBtn');
    const favoriteBtn = document.getElementById('favoriteBtn');
    
    // Play/Pause functionality
    if (playPauseBtn) {
        playPauseBtn.addEventListener('click', function() {
            isPlaying = !isPlaying;
            updatePlayPauseButton();
            
            if (isPlaying && currentTrack) {
                // Show toast with current track
                if (window.showToast) {
                    window.showToast(`Now playing: ${currentTrack.title}`, 'success');
                }
            }
        });
    }
    
    // Progress bar interaction
    if (progressBar) {
        progressBar.addEventListener('click', function(e) {
            const rect = progressBar.getBoundingClientRect();
            const clickPosition = (e.clientX - rect.left) / rect.width;
            currentTime = clickPosition * duration;
            updateProgress();
        });
    }
    
    // Volume control
    if (volumeSlider) {
        volumeSlider.addEventListener('input', function() {
            volume = parseInt(this.value);
            updateVolumeIcon();
        });
    }
    
    if (volumeBtn) {
        volumeBtn.addEventListener('click', function() {
            if (volume > 0) {
                volumeSlider.value = 0;
                volume = 0;
            } else {
                volumeSlider.value = 70;
                volume = 70;
            }
            updateVolumeIcon();
        });
    }
    
    // Shuffle button
    if (shuffleBtn) {
        shuffleBtn.addEventListener('click', function() {
            this.classList.toggle('active');
            if (window.showToast) {
                const isActive = this.classList.contains('active');
                window.showToast(`Shuffle ${isActive ? 'on' : 'off'}`, 'info');
            }
        });
    }
    
    // Repeat button
    if (repeatBtn) {
        repeatBtn.addEventListener('click', function() {
            this.classList.toggle('active');
            if (window.showToast) {
                const isActive = this.classList.contains('active');
                window.showToast(`Repeat ${isActive ? 'on' : 'off'}`, 'info');
            }
        });
    }
    
    // Favorite button
    if (favoriteBtn) {
        favoriteBtn.addEventListener('click', function() {
            const icon = this.querySelector('i');
            const isLiked = icon.classList.contains('fas');
            
            if (isLiked) {
                icon.classList.remove('fas', 'fa-heart');
                icon.classList.add('far', 'fa-heart');
                if (window.showToast) {
                    window.showToast('Removed from favorites', 'info');
                }
            } else {
                icon.classList.remove('far', 'fa-heart');
                icon.classList.add('fas', 'fa-heart');
                icon.style.color = '#e91e63';
                if (window.showToast) {
                    window.showToast('Added to favorites', 'success');
                }
            }
        });
    }
    
    // Functions
    function updatePlayPauseButton() {
        if (playPauseIcon) {
            if (isPlaying) {
                playPauseIcon.className = 'fas fa-pause';
                playPauseBtn.title = 'Pause';
            } else {
                playPauseIcon.className = 'fas fa-play';
                playPauseBtn.title = 'Play';
            }
        }
    }
    
    function updateProgress() {
        if (progressFill && duration > 0) {
            const percentage = (currentTime / duration) * 100;
            progressFill.style.width = percentage + '%';
        }
        
        if (currentTimeEl) {
            currentTimeEl.textContent = formatTime(currentTime);
        }
        
        if (totalTimeEl) {
            totalTimeEl.textContent = formatTime(duration);
        }
    }
    
    function updateVolumeIcon() {
        if (volumeIcon) {
            if (volume === 0) {
                volumeIcon.className = 'fas fa-volume-mute';
            } else if (volume < 50) {
                volumeIcon.className = 'fas fa-volume-down';
            } else {
                volumeIcon.className = 'fas fa-volume-up';
            }
        }
    }
    
    function formatTime(seconds) {
        const mins = Math.floor(seconds / 60);
        const secs = Math.floor(seconds % 60);
        return `${mins}:${secs.toString().padStart(2, '0')}`;
    }
    
    // Public functions for external use
    window.musicPlayer = {
        play: function(track) {
            currentTrack = track;
            duration = track.duration || 180; // Default 3 minutes
            currentTime = 0;
            isPlaying = true;
            
            // Update UI
            const titleEl = document.getElementById('currentTrackTitle');
            const artistEl = document.getElementById('currentTrackArtist');
            
            if (titleEl) titleEl.textContent = track.title || 'Unknown Track';
            if (artistEl) artistEl.textContent = track.artist || 'Unknown Artist';
            
            updatePlayPauseButton();
            updateProgress();
            
            // Show player
            bottomPlayer.style.display = 'flex';
            
            // Update main content padding
            updateMainContentPadding();
            
            // Simulate progress (for demo)
            if (window.musicPlayerInterval) {
                clearInterval(window.musicPlayerInterval);
            }
            
            window.musicPlayerInterval = setInterval(() => {
                if (isPlaying && currentTime < duration) {
                    currentTime += 1;
                    updateProgress();
                } else if (currentTime >= duration) {
                    // Track ended
                    isPlaying = false;
                    currentTime = 0;
                    updatePlayPauseButton();
                    updateProgress();
                    clearInterval(window.musicPlayerInterval);
                }
            }, 1000);
        },
        
        pause: function() {
            isPlaying = false;
            updatePlayPauseButton();
        },
        
        stop: function() {
            isPlaying = false;
            currentTime = 0;
            updatePlayPauseButton();
            updateProgress();
            if (window.musicPlayerInterval) {
                clearInterval(window.musicPlayerInterval);
            }
        }
    };
}

// ===== SIDEBAR FUNCTIONALITY =====

function initializeSidebar() {
    const sidebar = document.getElementById('musicSidebar');
    const sidebarToggleMobile = document.getElementById('sidebarToggleMobile');
    const sidebarBackdrop = document.getElementById('sidebarBackdrop');
    
    if (sidebarToggleMobile && sidebar) {
        sidebarToggleMobile.addEventListener('click', function() {
            sidebar.classList.toggle('open');
            if (sidebarBackdrop) {
                sidebarBackdrop.style.display = sidebar.classList.contains('open') ? 'block' : 'none';
            }
        });
    }
}

function updateSidebarLayout() {
    const sidebar = document.getElementById('musicSidebar');
    const mainContent = document.getElementById('mainContent');
    
    if (sidebar && mainContent && window.innerWidth >= 1024) {
        // Desktop: show sidebar and adjust main content
        sidebar.classList.add('open');
        mainContent.style.marginLeft = '240px';
    } else if (mainContent) {
        // Mobile/tablet: hide sidebar and reset main content
        if (sidebar) sidebar.classList.remove('open');
        mainContent.style.marginLeft = '0';
    }
}

function closeMobileSidebar() {
    const sidebar = document.getElementById('musicSidebar');
    const sidebarBackdrop = document.getElementById('sidebarBackdrop');
    
    if (sidebar) sidebar.classList.remove('open');
    if (sidebarBackdrop) sidebarBackdrop.style.display = 'none';
}

function updateMainContentPadding() {
    const mainContent = document.getElementById('mainContent');
    const main = mainContent?.querySelector('main');
    
    if (main) {
        const bottomPlayer = document.getElementById('bottomPlayer');
        const isPlayerVisible = bottomPlayer && bottomPlayer.style.display === 'flex';
        const bottomPadding = isPlayerVisible ? '90px' : '24px';
        
        main.style.paddingBottom = bottomPadding;
    }
}

// Add resize event listener
window.addEventListener('resize', debounce(function() {
    updateSidebarLayout();
}, 250));

// Add click handlers for music items
document.addEventListener('click', function(e) {
    const musicItem = e.target.closest('.music-item');
    if (musicItem && window.musicPlayer) {
        // Extract track info from the element
        const title = musicItem.querySelector('.track-title')?.textContent || 'Unknown Track';
        const artist = musicItem.querySelector('.track-artist')?.textContent || 'Unknown Artist';
        const duration = 180; // Default duration
        
        // Remove previous playing states
        document.querySelectorAll('.music-item.playing').forEach(item => {
            item.classList.remove('playing');
        });
        
        // Add playing state to current item
        musicItem.classList.add('playing');
        
        // Play the track
        window.musicPlayer.play({ title, artist, duration });
    }
});

// ===== SPOTIFY-LIKE ENHANCEMENTS =====

// Enhanced Card Interactions
document.addEventListener('DOMContentLoaded', function() {
    // Add Spotify-like card hover effects
    const cards = document.querySelectorAll('.card, .spotify-card');
    cards.forEach(card => {
        card.addEventListener('mouseenter', function() {
            if (!this.classList.contains('no-hover')) {
                this.style.transform = 'translateY(-2px)';
                this.style.transition = 'all 0.2s ease';
            }
        });
        
        card.addEventListener('mouseleave', function() {
            if (!this.classList.contains('no-hover')) {
                this.style.transform = 'translateY(0)';
            }
        });
    });
    
    // Enhanced button hover effects
    const buttons = document.querySelectorAll('.btn');
    buttons.forEach(button => {
        button.addEventListener('mouseenter', function() {
            if (!this.disabled && !this.classList.contains('loading')) {
                this.style.transform = 'translateY(-1px)';
                this.style.transition = 'all 0.2s ease';
            }
        });
        
        button.addEventListener('mouseleave', function() {
            if (!this.disabled) {
                this.style.transform = 'translateY(0)';
            }
        });
    });
    
    // Spotify-like focus management
    document.addEventListener('keydown', function(e) {
        // Escape key handling
        if (e.key === 'Escape') {
            // Close any open dropdowns
            const openDropdowns = document.querySelectorAll('.dropdown-menu.show');
            openDropdowns.forEach(dropdown => {
                const toggle = dropdown.previousElementSibling;
                if (toggle) {
                    bootstrap.Dropdown.getInstance(toggle)?.hide();
                }
            });
            
            // Close modals
            const openModals = document.querySelectorAll('.modal.show');
            openModals.forEach(modal => {
                bootstrap.Modal.getInstance(modal)?.hide();
            });
        }
        
        // Keyboard shortcuts
        if (e.ctrlKey || e.metaKey) {
            switch(e.key) {
                case 'k':
                    e.preventDefault();
                    // Focus search if it exists
                    const searchInput = document.querySelector('.search-input, input[type="search"]');
                    if (searchInput) {
                        searchInput.focus();
                        searchInput.select();
                    }
                    break;
                case 'ArrowLeft':
                    e.preventDefault();
                    // Previous track
                    if (window.musicPlayer) {
                        const prevBtn = document.getElementById('prevBtn');
                        if (prevBtn) prevBtn.click();
                    }
                    break;
                case 'ArrowRight':
                    e.preventDefault();
                    // Next track
                    if (window.musicPlayer) {
                        const nextBtn = document.getElementById('nextBtn');
                        if (nextBtn) nextBtn.click();
                    }
                    break;
                case ' ':
                    e.preventDefault();
                    // Play/pause
                    if (window.musicPlayer) {
                        const playPauseBtn = document.getElementById('playPauseBtn');
                        if (playPauseBtn) playPauseBtn.click();
                    }
                    break;
            }
        }
    });
    
    // Enhanced loading states
    const forms = document.querySelectorAll('form');
    forms.forEach(form => {
        form.addEventListener('submit', function() {
            const submitBtn = this.querySelector('button[type="submit"], input[type="submit"]');
            if (submitBtn && !submitBtn.disabled) {
                submitBtn.classList.add('loading');
                submitBtn.disabled = true;
                
                const originalText = submitBtn.innerHTML;
                const loadingText = submitBtn.dataset.loadingText || 'Loading...';
                
                if (!submitBtn.querySelector('.loading-spinner')) {
                    submitBtn.innerHTML = `<span class="loading-spinner me-2"></span>${loadingText}`;
                }
                
                // Reset after timeout as fallback
                setTimeout(() => {
                    submitBtn.classList.remove('loading');
                    submitBtn.disabled = false;
                    submitBtn.innerHTML = originalText;
                }, 10000);
            }
        });
    });
    
    // Smooth scroll for internal links
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function (e) {
            e.preventDefault();
            const target = document.querySelector(this.getAttribute('href'));
            if (target) {
                target.scrollIntoView({
                    behavior: 'smooth',
                    block: 'start'
                });
            }
        });
    });
    
    // Auto-hide alerts
    const alerts = document.querySelectorAll('.alert:not(.alert-permanent)');
    alerts.forEach(alert => {
        if (!alert.querySelector('.btn-close')) {
            setTimeout(() => {
                alert.style.opacity = '0';
                alert.style.transform = 'translateX(100%)';
                setTimeout(() => {
                    if (alert.parentNode) {
                        alert.parentNode.removeChild(alert);
                    }
                }, 300);
            }, 5000);
        }
    });
    
    // Enhanced tooltip functionality
    const tooltipElements = document.querySelectorAll('[data-bs-toggle="tooltip"], [title]:not([data-bs-toggle])');
    tooltipElements.forEach(element => {
        if (!element.hasAttribute('data-bs-toggle')) {
            element.setAttribute('data-bs-toggle', 'tooltip');
        }
        new bootstrap.Tooltip(element, {
            delay: { show: 500, hide: 100 }
        });
    });
});

// Utility function for showing notifications
window.showNotification = function(message, type = 'info', duration = 3000) {
    const notification = document.createElement('div');
    notification.className = `alert alert-${type} alert-dismissible fade show position-fixed`;
    notification.style.cssText = `
        top: 20px;
        right: 20px;
        z-index: 1060;
        min-width: 300px;
        max-width: 500px;
        box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
    `;
    
    notification.innerHTML = `
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;
    
    document.body.appendChild(notification);
    
    // Auto dismiss
    setTimeout(() => {
        if (notification.parentNode) {
            notification.classList.remove('show');
            setTimeout(() => {
                if (notification.parentNode) {
                    notification.parentNode.removeChild(notification);
                }
            }, 150);
        }
    }, duration);
};

// Enhanced error handling
window.addEventListener('error', function(e) {
    console.error('JavaScript error:', e.error);
    if (window.showNotification) {
        window.showNotification('An error occurred. Please refresh the page.', 'danger');
    }
});

// Performance monitoring
if (window.performance && window.performance.mark) {
    window.addEventListener('load', function() {
        setTimeout(() => {
            const perfData = window.performance.getEntriesByType('navigation')[0];
            if (perfData) {
                console.log(`Page load time: ${Math.round(perfData.loadEventEnd - perfData.loadEventStart)}ms`);
            }
        }, 0);
    });
}
