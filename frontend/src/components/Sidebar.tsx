import React from 'react';
import { Link, useLocation } from 'react-router-dom';

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
  ),
};

const Sidebar: React.FC<{ onLogout: () => void }> = ({ onLogout }) => {
  const location = useLocation();

  const isActive = (path: string) => location.pathname === path;

  return (
    <nav className="bg-gray-900 w-64 p-6 flex flex-col h-screen sticky top-0 border-r border-gray-700">
      <h2 className="text-3xl font-extrabold text-blue-500 mb-8">SM App</h2>
      <Link
        to="/dashboard"
        className={`flex items-center space-x-4 p-3 rounded-full ${
          isActive('/dashboard') ? 'bg-blue-600 text-white' : 'text-gray-400 hover:bg-gray-800'
        }`}
      >
        {icons.home}
        <span className="font-semibold">Explore</span>
      </Link>
      <Link
        to="/dashboard/profile"
        className={`flex items-center space-x-4 p-3 rounded-full ${
          isActive('/dashboard/profile') ? 'bg-blue-600 text-white' : 'text-gray-400 hover:bg-gray-800'
        }`}
      >
        {icons.profile}
        <span className="font-semibold">Profile</span>
      </Link>
      <Link
        to="/dashboard/create"
        className={`flex items-center space-x-4 p-3 rounded-full ${
          isActive('/dashboard/create') ? 'bg-blue-600 text-white' : 'text-gray-400 hover:bg-gray-800'
        }`}
      >
        {icons.create}
        <span className="font-semibold">Make a Post</span>
      </Link>
      <Link
        to="/dashboard/search"
        className={`flex items-center space-x-4 p-3 rounded-full ${
          isActive('/dashboard/search') ? 'bg-blue-600 text-white' : 'text-gray-400 hover:bg-gray-800'
        }`}
      >
        {icons.search}
        <span className="font-semibold">Search</span>
      </Link>

      <div className="mt-auto">
        <button
          onClick={onLogout}
          className="flex items-center space-x-4 p-3 rounded-full text-gray-400 hover:bg-gray-800 w-full text-left"
        >
          <span className="font-semibold">Logout</span>
        </button>
      </div>
    </nav>
  );
};

export default Sidebar;
