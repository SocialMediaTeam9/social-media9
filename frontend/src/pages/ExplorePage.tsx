import React from 'react';
import Post from '../components/Post';

// --- Page Components ---
// This component displays a list of posts for the explore feed.
const ExplorePage: React.FC = () => (
  <div className="feed-content">
    <Post author="" username="johndoe" content="A beautiful day for coding! Excited to share my new project with everyone soon. #React #TypeScript" />
    <Post author="Jane Smith" username="janesmith" content="Just finished a great workout. Feeling refreshed and ready for the week! 💪 #fitness" />
    <Post author="Code Master" username="codemaster" content="Having a great time building my social app. It's a lot of work but it's going to be so worth it!" />
  </div>
);

export default ExplorePage;
