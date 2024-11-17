/** @type {import('tailwindcss').Config} */
export default {
    content: [
        './Pages/**/*.razor',
        '../DiscussionForum.Client/Components/**/*.razor'
    ],
    theme: {
        extend: {
            keyframes: {
                "fade-in-down": {
                    "0%": {
                        opacity: "0",
                        translate: "0 -100%",
                    },
                    "100%": {
                        opacity: "1",
                        translate: "0",
                    },
                },
            },
            animation: {
                'fade-in-down': 'fade-in-down 0.5s ease-out',
            }
        },
    },
    plugins: [],
}
