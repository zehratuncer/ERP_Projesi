# 1. Build Stage (Node.js)
FROM node:20-alpine AS build
WORKDIR /app

COPY package*.json ./
RUN npm ci

COPY . .
RUN npm run build -- --configuration production

# 2. Production Stage (Nginx)
FROM nginx:alpine
COPY --from=build /app/dist/frontend/browser /usr/share/nginx/html
# Fallback in case browser subfolder doesn't exist
# COPY --from=build /app/dist/frontend /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
