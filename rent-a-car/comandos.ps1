az login
az acr login --name acrlab007alexoliveira --resource-group LAB007

docker tag bff-rent-a-car-local acrlab007alexoliveira.azurecr.io/bff-rent-a-car-local:v1
docker push acrlab007alexoliveira.azurecr.io/bff-rent-a-car-local:v1

az containerapp env create --name bff-rent-a-car-local --resource-group LAB007 --location eastus

az containerapp create --name bff-rent-a-car-local --resource-group LAB007 --environment bff-rent-a-car-local --image acrlab007alexoliveira.azurecr.io/bff-rent-a-car-local:v1 --target-port 3001 --ingress 'external' --registry-server acrlab007alexoliveira.azurecr.io






