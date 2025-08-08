import React from 'react';

interface HeartIconProps {
    filled?: boolean;
    className?: string;
}

const HeartIcon: React.FC<HeartIconProps> = ({ filled = false, className = "w-5 h-5" }) => {
    return (
        <svg
            className={className}
            fill={filled ? "currentColor" : "none"}
            stroke="currentColor"
            viewBox="0 0 24 24"
            xmlns="http://www.w3.org/2000/svg"
        >
            <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M4.318 6.318a4.5 4.5 0 000 6.364L12 20.364l7.682-7.682a4.5 4.5 0 00-6.364-6.364L12 7.636l-1.318-1.318a4.5 4.5 0 00-6.364 0z"
            />
        </svg>
    );
};

export default HeartIcon;
