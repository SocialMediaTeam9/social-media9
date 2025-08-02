import React from 'react';

// --- SVG Icons ---
// These icons are included here to keep the component self-contained.
// In a larger project, they might be in a separate utility file.
const icons = {
  home: (
    <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
      <path d="M12 2L2 12H5V21H19V12H22L12 2ZM12 4.35L19 11.35V19H15V12H9V19H5V11.35L12 4.35Z" fill="currentColor"/>
    </svg>
  ),
  profile: (
    <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
      <path d="M12 12C14.7614 12 17 9.76142 17 7C17 4.23858 14.7614 2 12 2C9.23858 2 7 4.23858 7 7C7 9.76142 9.23858 12 12 12ZM12 14C8.68629 14 6 16.6863 6 20C6 20.5523 6.44772 21 7 21H17C17.5523 21 18 20.5523 18 20C18 16.6863 15.3137 14 12 14Z" fill="currentColor"/>
    </svg>
  ),
  create: (
    <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
      <path d="M19 12H13V18H11V12H5V10H11V4H13V10H19V12Z" fill="currentColor"/>
    </svg>
  ),
  search: (
    <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
      <path d="M10 18C14.4183 18 18 14.4183 18 10C18 5.58172 14.4183 2 10 2C5.58172 2 2 5.58172 2 10C2 14.4183 5.58172 18 10 18ZM16.4951 16.4951L21.0001 21L19.5859 22.4142L15.0809 17.9092L16.4951 16.4951Z" fill="currentColor"/>
    </svg>
  )
};

// Define the available pages in the app
type PageType = 'explore' | 'profile' | 'createPost' | 'search';

// Define the props for the Sidebar component
interface SidebarProps {
  currentPage: PageType;
  onPageChange: (page: PageType) => void;
  onLogout: () => void;
}

// Sidebar component that handles navigation and logout
const Sidebar: React.FC<SidebarProps> = ({ currentPage, onPageChange, onLogout }) => (
  <nav className="bg-gray-900 w-64 p-6 flex flex-col h-screen sticky top-0 border-r border-gray-700">
    <h2 className="text-3xl font-extrabold text-blue-500 mb-8">SocialMediaApp</h2>
    
    {/* Navigation Buttons */}
    <button
      className={`flex items-center space-x-4 p-3 rounded-full transition-colors duration-200 ${
        currentPage === 'explore' ? 'bg-blue-600 text-white' : 'text-gray-400 hover:bg-gray-800'
      }`}
      onClick={() => onPageChange('explore')}
    >
      {icons.home}
      <span className="font-semibold">Explore</span>
    </button>
    <button
      className={`flex items-center space-x-4 p-3 rounded-full transition-colors duration-200 ${
        currentPage === 'profile' ? 'bg-blue-600 text-white' : 'text-gray-400 hover:bg-gray-800'
      }`}
      onClick={() => onPageChange('profile')}
    >
      {icons.profile}
      <span className="font-semibold">Profile</span>
    </button>
    <button
      className={`flex items-center space-x-4 p-3 rounded-full transition-colors duration-200 ${
        currentPage === 'createPost' ? 'bg-blue-600 text-white' : 'text-gray-400 hover:bg-gray-800'
      }`}
      onClick={() => onPageChange('createPost')}
    >
      {icons.create}
      <span className="font-semibold">Make a Post</span>
    </button>
    <button
      className={`flex items-center space-x-4 p-3 rounded-full transition-colors duration-200 ${
        currentPage === 'search' ? 'bg-blue-600 text-white' : 'text-gray-400 hover:bg-gray-800'
      }`}
      onClick={() => onPageChange('search')}
    >
      {icons.search}
      <span className="font-semibold">Search</span>
    </button>

    {/* Logout Button */}
    <div className="mt-auto">
      <button
        onClick={onLogout}
        className="flex items-center space-x-4 p-3 rounded-full transition-colors duration-200 text-gray-400 hover:bg-gray-800 w-full text-left"
      >
        <span className="font-semibold">Logout</span>
      </button>
    </div>
  </nav>
);

export default Sidebar;
