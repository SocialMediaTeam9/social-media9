import React, { useState } from 'react';
import { fetcher } from './utils/fetcher'; // Assuming fetcher is in this path


// --- SVG Icons (using a simple object for easy use) ---
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

// --- Login Page Component ---
const LoginPage: React.FC<{ onLogin: () => void }> = ({ onLogin }) => {
  // State to manage the loading status and any login errors
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Async function to handle the Google login click event
  const handleLoginClick = async () => {
    setIsLoading(true);
    setError(null);

    try {
      // Simulate calling the Google login endpoint via the fetcher utility
      await fetcher('/google-login', { method: 'POST' });

      // On successful login, call the onLogin prop from the parent
      onLogin();

    } catch (err: any) {
      console.error("Login failed:", err);
      setError('Login failed. Please try again.');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-gray-900 flex items-center justify-center p-6">
      <div className="bg-gray-800 p-10 rounded-2xl shadow-2xl text-center max-w-sm w-full">
        <h1 className="text-4xl font-extrabold text-blue-500 mb-2">SM App</h1>
        <p className="text-gray-400 mb-8">Sign in to access your dashboard.</p>
        <button
          onClick={handleLoginClick}
          disabled={isLoading}
          className="w-full bg-blue-600 hover:bg-blue-700 text-white font-bold py-3 px-6 rounded-full transition-all duration-300 transform hover:scale-105 disabled:bg-gray-500 disabled:hover:scale-100 disabled:cursor-not-allowed"
        >
          {isLoading ? 'Signing in...' : 'Sign in with Google'}
        </button>
        {error && (
          <div className="mt-4 text-red-400 text-sm font-medium">
            {error}
          </div>
        )}
      </div>
    </div>
  );
};

// --- Post Component (placeholder) ---
const Post: React.FC<{ author: string; username: string; content: string }> = ({ author, username, content }) => (
  <div className="border-b border-gray-700 p-4 flex gap-4 transition-colors hover:bg-gray-800">
    <div className="w-12 h-12 bg-gray-600 rounded-full flex-shrink-0"></div>
    <div className="flex-1">
      <div className="flex items-center space-x-2">
        <span className="font-bold text-white">{author}</span>
        <span className="text-gray-500">@{username}</span>
      </div>
      <p className="text-gray-300 mt-1">{content}</p>
    </div>
  </div>
);

// --- Page Components (placeholders) ---
const ExplorePage: React.FC = () => (
  <div className="feed-content">
    <Post author="John Doe" username="johndoe" content="A beautiful day for coding! Excited to share my new project with everyone soon. #React #TypeScript" />
    <Post author="Jane Smith" username="janesmith" content="Just finished a great workout. Feeling refreshed and ready for the week! 💪 #fitness" />
    <Post author="Code Master" username="codemaster" content="Having a great time building my social app. It's a lot of work but it's going to be so worth it!" />
  </div>
);

const ProfilePage: React.FC = () => (
  <div className="p-6 bg-gray-800 rounded-lg shadow-xl">
    <h2 className="text-2xl font-bold text-white">My Profile</h2>
    <p className="text-gray-400 mt-2">This is your profile page. It will display your information and your posts.</p>
  </div>
);

const CreatePostPage: React.FC = () => (
  <div className="p-6 bg-gray-800 rounded-lg shadow-xl">
    <h2 className="text-2xl font-bold text-white">Create a Post</h2>
    <p className="text-gray-400 mt-2">This is the create post page. You can share your thoughts here.</p>
  </div>
);

const SearchPage: React.FC = () => (
  <div className="p-6 bg-gray-800 rounded-lg shadow-xl">
    <h2 className="text-2xl font-bold text-white">Search</h2>
    <p className="text-gray-400 mt-2">This is the search page. You can search for users and posts here.</p>
  </div>
);

// --- Sidebar Component (placeholder) ---
type PageType = 'explore' | 'profile' | 'createPost' | 'search';

interface SidebarProps {
  currentPage: PageType;
  onPageChange: (page: PageType) => void;
  onLogout: () => void;
}

const Sidebar: React.FC<SidebarProps> = ({ currentPage, onPageChange, onLogout }) => (
  <nav className="bg-gray-900 w-64 p-6 flex flex-col h-screen sticky top-0 border-r border-gray-700">
    <h2 className="text-3xl font-extrabold text-blue-500 mb-8">SM App</h2>
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

// --- Dashboard Component ---
const Dashboard: React.FC<{ onLogout: () => void }> = ({ onLogout }) => {
  const [currentPage, setCurrentPage] = useState<PageType>('explore');

  const renderPage = () => {
    switch (currentPage) {
      case 'explore':
        return <ExplorePage />;
      case 'profile':
        return <ProfilePage />;
      case 'createPost':
        return <CreatePostPage />;
      case 'search':
        return <SearchPage />;
      default:
        return <ExplorePage />;
    }
  };

  const getPageTitle = () => {
    switch (currentPage) {
      case 'explore':
        return 'Explore';
      case 'profile':
        return 'Profile';
      case 'createPost':
        return 'Create a Post';
      case 'search':
        return 'Search';
      default:
        return 'Explore';
    }
  };

  return (
    <div className="flex min-h-screen">
      <Sidebar currentPage={currentPage} onPageChange={setCurrentPage} onLogout={onLogout} />
      <main className="flex-1 flex flex-col border-r border-gray-700">
        <div className="p-6 text-2xl font-bold border-b border-gray-700 sticky top-0 bg-gray-900 z-10">{getPageTitle()}</div>
        <div className="flex-1 overflow-y-auto">
          {renderPage()}
        </div>
      </main>
      <aside className="w-80 p-6 hidden lg:block">
        <div className="bg-gray-800 rounded-lg p-4">
          <h3 className="text-xl font-bold mb-4">Who to Follow</h3>
          <p className="text-gray-400">Follow a user to see their posts!</p>
        </div>
      </aside>
    </div>
  );
};


// --- Main App Component ---
// This is the top-level component that controls the application flow.
const App: React.FC = () => {
  // Use state to track whether the user is logged in.
  const [isLoggedIn, setIsLoggedIn] = useState<boolean>(false);

  // Function to handle the login action.
  const handleLogin = () => setIsLoggedIn(true);
  
  // Function to handle the logout action.
  const handleLogout = () => setIsLoggedIn(false);

  return (
    <>
      {/* Conditionally render the login page or the dashboard based on authentication state */}
      {isLoggedIn ? (
        <Dashboard onLogout={handleLogout} />
      ) : (
        <LoginPage onLogin={handleLogin} />
      )}
    </>
  );
};

export default App;
