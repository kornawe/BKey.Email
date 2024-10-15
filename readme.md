# Serverless Email Server Setup

This guide provides instructions for setting up a serverless email server using AWS services like Simple Email Service (SES), S3, and Lambda to build the foundation for an email processing solution that supports IMAP functionality.

## Prerequisites
- An AWS account with administrator privileges
- AWS CLI configured with appropriate access credentials
- Basic understanding of AWS services (SES, S3, Lambda, IAM)

## Resources Created by CloudFormation
- **S3 Bucket**: Stores emails received via AWS SES
- **IAM Role**: Allows Lambda to read/write to the S3 bucket
- **SES Rule Set and Receipt Rule**: Configures SES to receive incoming emails and store them in S3
- **API Gateway**: Exposes Lambda functions as RESTful endpoints to mimic IMAP commands
- **AWS Cognito User Pool**: Manages user authentication and authorization

## Steps to Deploy

### 1. Clone the Repository
If you haven't done so already, clone the repository where the CloudFormation template is stored:
```sh
git clone <repository_url>
cd <repository_directory>
```

### 2. Update Parameters in the Template
Open the `cloudformation.yaml` file and customize the following parameters to suit your needs:
- **EnvironmentName**: Set an environment name (e.g., `prod`, `dev`). This will be appended to resources to differentiate environments.
- **Recipients**: Modify the recipient email addresses in the SES Receipt Rule to match your verified domain.

### 3. Deploy CloudFormation Stack
Use the AWS CLI or AWS Console to deploy the CloudFormation stack.

#### Using AWS CLI
```sh
aws cloudformation deploy \
  --template-file cloudformation.yaml \
  --stack-name serverless-email-stack \
  --parameter-overrides EnvironmentName=<your_environment_name>
```
Make sure to replace `<your_environment_name>` with the actual environment name you chose.

### 4. Verify SES Configuration
- **Verify Domain**: Ensure that your domain is verified in SES for receiving emails. Go to the [AWS SES Console](https://console.aws.amazon.com/ses/home) and verify the domain if it's not already.
- **Add MX Record**: Update the DNS configuration of your domain to add an MX record pointing to AWS SES. This allows emails to be routed through AWS.

### 5. Testing the Setup
- **Send Test Emails**: Send a test email to the address defined in the SES Receipt Rule. The email should be stored in the S3 bucket under the prefix `emails/`.
- **Check S3**: Navigate to the S3 bucket defined by the stack and ensure that the email appears in the bucket.

## Next Steps: Adding IMAP Functionality
The current setup supports storing incoming emails in an S3 bucket. To add IMAP functionality, follow these steps:

1. **Create Lambda Functions**
   - Develop Lambda functions that can read emails from the S3 bucket and interact with DynamoDB for metadata management.
   - These functions will handle IMAP commands like `LIST`, `FETCH`, and `STORE`.

2. **API Gateway Integration**
   - Expose Lambda functions via API Gateway to allow IMAP clients to communicate with your server.
   - Configure endpoints to handle different IMAP commands, such as authentication, listing mailboxes, and fetching email content.

3. **Authentication Using Cognito**
   - Integrate AWS Cognito for user management and secure authentication.
   - Create a Cognito User Pool to handle user registration and authentication.
   - Configure API Gateway to use Cognito Authorizer for securing access to the endpoints.

## Cleanup
If you need to delete the stack, you can use the following command to remove all associated resources:
```sh
aws cloudformation delete-stack --stack-name serverless-email-stack
```

Ensure all emails in the S3 bucket are backed up or no longer needed before running this command, as deleting the stack will delete the bucket and all its contents.

## Important Notes
- **Cost Considerations**: AWS SES and S3 incur costs based on usage. Monitor your usage to avoid unexpected expenses.
- **Security**: Always secure email access with appropriate permissions. Limit who can access the Lambda functions, S3 bucket, and API Gateway endpoints.

## Estimated Costs
Below is an estimate of the costs you may incur while setting up and using this serverless email server:

- **Domain Registration**: A `.com` domain is approximately $14.00 per year through Route 53.
- **AWS SES**:
  - **Domain Verification**: Free
  - **Incoming Emails**: $0.10 per 1,000 emails received
  - **Outgoing Emails**: $0.10 per 1,000 emails sent (additional charges may apply if not in the free tier)
- **S3 Storage**: $0.023 per GB per month for standard storage
- **Lambda Invocations**: $0.20 per 1 million requests (first 1 million requests are free per month)
- **API Gateway**: $3.50 per million API calls (if used for IMAP implementation)
- **Cognito**: $0.0055 per MAU (Monthly Active User) for authentication

These costs are estimates and can vary depending on your actual usage. Always monitor your AWS billing to keep track of the costs.

## Contact
If you encounter issues or have questions, please reach out to the repository maintainer or open an issue.